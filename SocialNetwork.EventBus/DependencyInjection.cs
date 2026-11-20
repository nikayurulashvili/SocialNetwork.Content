using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SocialNetwork.EventBus.Connection;
using SocialNetwork.EventBus.Consuming;
using SocialNetwork.EventBus.Outbox;

namespace SocialNetwork.EventBus;

public static class DependencyInjection
{
    public static IServiceCollection AddOutboxPublisher<TStore>(this IServiceCollection services, string exchange)
        where TStore : class, IOutboxStore
    {
        services.TryAddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();

        services.Configure<OutboxOptions>(o => o.Exchange = exchange);
        services.AddScoped<IOutboxStore, TStore>();
        services.AddHostedService<OutboxPublisher>();
        return services;
    }

    public static IServiceCollection AddConsumer<TConsumer>(this IServiceCollection services)
        where TConsumer : RabbitMqConsumer
    {
        services.TryAddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();

        services.AddHostedService<TConsumer>();
        return services;
    }
}