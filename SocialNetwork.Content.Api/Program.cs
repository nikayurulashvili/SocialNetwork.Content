using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using SocialNetwork.Content.Api.Consumers;
using SocialNetwork.Content.Repositories.Persistence;
using SocialNetwork.Content.Repositories.Persistence.EventBus;
using SocialNetwork.Content.Repositories.Persistence.Repositories;
using SocialNetwork.Content.Services.Abstractions.Persistence;
using SocialNetwork.Content.Services.Abstractions.Services;
using SocialNetwork.Content.Services.Services;
using SocialNetwork.EventBus;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        // Database
        var contentConnection = builder.Configuration.GetConnectionString("ContentDb")
            ?? throw new InvalidOperationException("ConnectionStrings:ContentDb is missing.");
        builder.Services.AddDbContext<ContentDbContext>(options => options.UseSqlServer(contentConnection));

        // Content services and repositories
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IPostService, PostService>();
        builder.Services.AddScoped<ICommentService, CommentService>();
        builder.Services.AddScoped<ILikeService, LikeService>();
        builder.Services.AddScoped<IUserEventsService, UserEventsService>();

        // RabbitMQ connection factory (the EventBus connection provider manages the connection lifetime).
        var rabbit = builder.Configuration.GetSection("RabbitMq");
        builder.Services.AddSingleton<RabbitMQ.Client.IConnectionFactory>(_ => new ConnectionFactory
        {
            HostName = rabbit["HostName"] ?? "localhost",
            Port = int.TryParse(rabbit["Port"], out var port) ? port : 5672,
            UserName = rabbit["UserName"] ?? "guest",
            Password = rabbit["Password"] ?? "guest",
            VirtualHost = rabbit["VirtualHost"] ?? "/",
            AutomaticRecoveryEnabled = true,
        });

        builder.Services.AddOutboxPublisher<ContentOutboxStore>("socialnetwork.events");
        builder.Services.AddConsumer<UserCreatedConsumer>();
        builder.Services.AddConsumer<UserUpdatedConsumer>();
        builder.Services.AddConsumer<UserDeletedConsumer>();

        var app = builder.Build();
        if (app.Environment.IsDevelopment()) app.MapOpenApi();
        app.UseHttpsRedirection();
        app.MapControllers();
        app.Run();
    }
}