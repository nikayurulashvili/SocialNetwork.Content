using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using SocialNetwork.EventBus.Connection;

namespace SocialNetwork.EventBus.Outbox;
public sealed class OutboxPublisher(
    IRabbitMqConnectionProvider connections,
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxOptions> options,
    ILogger<OutboxPublisher> logger) : BackgroundService
{
    private readonly OutboxOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_options.PollInterval);
        IChannel? channel = null;

        try
        {
            do
            {
                try
                {
                    channel ??= await OpenChannelAsync(stoppingToken);

                    int count;
                    do
                    {
                        count = await PublishBatchAsync(channel, stoppingToken);
                    }
                    while (count == _options.BatchSize);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Outbox publishing failed; retrying on next tick.");
                    if (channel is not null)
                    {
                        await SafeDisposeAsync(channel);
                        channel = null;
                    }
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) 
        {
            // აპი ითიშება: ნორმალური გაჩერებაა, შეცდომა არ არის.
        }
        finally
        {
            if (channel is not null)
            {
                await SafeDisposeAsync(channel);
            }
        }
    }

    private async Task<IChannel> OpenChannelAsync(CancellationToken ct)
    {
        var connection = await connections.GetConnectionAsync(ct);

        var channel = await connection.CreateChannelAsync(
                new CreateChannelOptions(
                        publisherConfirmationsEnabled: true,
                        publisherConfirmationTrackingEnabled: true
                    ),
            ct);

        await channel.ExchangeDeclareAsync(_options.Exchange, ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: ct);

        return channel;
    }

    private async Task<int> PublishBatchAsync(IChannel channel, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();

        var messages = await store.GetPendingAsync(_options.BatchSize, ct);

        try
        {
            foreach (var message in messages)
            {
                var properties = new BasicProperties
                {
                    MessageId = message.Id.ToString(),
                    Type = message.Type,
                    ContentType = "application/json",
                    DeliveryMode = DeliveryModes.Persistent
                };

                try
                {
                    await channel.BasicPublishAsync(
                        _options.Exchange, message.Type, mandatory: false, properties,
                        Encoding.UTF8.GetBytes(message.Payload), ct);

                    store.MarkPublished(message.Id);
                }
                catch (PublishException ex)
                {
                    store.MarkFailed(message.Id, ex.Message);
                }
            }
        }
        finally
        {
            await store.SaveChangesAsync(CancellationToken.None);
        }

        return messages.Count;
    }
    private async Task SafeDisposeAsync(IChannel channel)
    {
        try
        {
            await channel.DisposeAsync();
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to dispose RabbitMQ channel.");
        }
    }
}