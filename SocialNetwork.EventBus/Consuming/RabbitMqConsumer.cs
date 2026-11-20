using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SocialNetwork.EventBus.Connection;

namespace SocialNetwork.EventBus.Consuming;

public abstract class RabbitMqConsumer(
    IRabbitMqConnectionProvider connections,
    IServiceScopeFactory scopeFactory,
    ILogger logger) : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    protected abstract string QueueName { get; }
    protected abstract string Exchange { get; }
    protected abstract IReadOnlyCollection<string> RoutingKeys { get; }
    protected virtual ushort PrefetchCount => 10;

    protected abstract Task HandleAsync(
        IServiceProvider services, Guid messageId, string routingKey,
        ReadOnlyMemory<byte> body, CancellationToken ct);

    protected static T Deserialize<T>(ReadOnlyMemory<byte> body) =>
        JsonSerializer.Deserialize<T>(body.Span)
        ?? throw new JsonException($"Message body deserialized to null for {typeof(T).Name}.");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IChannel? channel = null;

        while (channel is null)
        {
            try
            {
                channel = await StartConsumingAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not start consumer for {Queue}; retrying in {Delay}.", QueueName, RetryDelay);
                try 
                { 
                    await Task.Delay(RetryDelay, stoppingToken); 
                }
                catch (OperationCanceledException) 
                { 
                    return; 
                }
            }
        }

        await using (channel)
        {
            try 
            { 
                await Task.Delay(Timeout.Infinite, stoppingToken); 
            }
            catch (OperationCanceledException) { }
        }
    }

    private async Task<IChannel> StartConsumingAsync(CancellationToken ct)
    {
        var connection = await connections.GetConnectionAsync(ct);
        var channel = await connection.CreateChannelAsync(cancellationToken: ct);

        try
        {
            await DeclareTopologyAsync(channel, ct);
            await channel.BasicQosAsync(prefetchSize: 0, PrefetchCount, global: false, ct);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += (_, ea) => OnReceivedAsync(channel, ea, ct);

            await channel.BasicConsumeAsync(QueueName, autoAck: false, consumer, ct);
            return channel;
        }
        catch
        {
            try
            {
                await channel.DisposeAsync();
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Failed to dispose RabbitMQ channel.");
            }

            throw;
        }
    }

    private async Task DeclareTopologyAsync(IChannel channel, CancellationToken ct)
    {
        var deadLetterExchange = $"{QueueName}.dlx";
        var deadLetterQueue = $"{QueueName}.dlq";

        await channel.ExchangeDeclareAsync(Exchange, ExchangeType.Topic, durable: true, cancellationToken: ct);
        await channel.ExchangeDeclareAsync(deadLetterExchange, ExchangeType.Fanout, durable: true, cancellationToken: ct);

        await channel.QueueDeclareAsync(deadLetterQueue, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
        await channel.QueueBindAsync(deadLetterQueue, deadLetterExchange, routingKey: string.Empty, cancellationToken: ct);

        await channel.QueueDeclareAsync(
            QueueName, durable: true, exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object?> { ["x-dead-letter-exchange"] = deadLetterExchange },
            cancellationToken: ct);

        foreach (var key in RoutingKeys)
        {
            await channel.QueueBindAsync(QueueName, Exchange, key, cancellationToken: ct);
        }
    }

    private async Task OnReceivedAsync(IChannel channel, BasicDeliverEventArgs ea, CancellationToken ct)
    {
        try
        {
            if (!Guid.TryParse(ea.BasicProperties.MessageId, out var messageId))
            {
                throw new InvalidOperationException($"Invalid MessageId '{ea.BasicProperties.MessageId}'.");
            }

            await using var scope = scopeFactory.CreateAsyncScope();
            await HandleAsync(scope.ServiceProvider, messageId, ea.RoutingKey, ea.Body, ct);

            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, CancellationToken.None);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // არ არის შეცდომა, აპი ითიშება.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process {RoutingKey} (MessageId: {MessageId}); sending to DLQ.",
                ea.RoutingKey, ea.BasicProperties.MessageId);

            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, CancellationToken.None);
        }
    }
}