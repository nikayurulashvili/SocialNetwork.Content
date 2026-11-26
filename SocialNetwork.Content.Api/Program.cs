using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using SocialNetwork.Content.Api.Consumers;
using SocialNetwork.Content.Contracts.Topology;
using SocialNetwork.Content.Repositories.Persistence;
using SocialNetwork.Content.Repositories.Persistence.Repositories;
using SocialNetwork.Content.Repositories.RabbitMq.Outbox;
using SocialNetwork.Content.Services.Abstractions.Persistence;
using SocialNetwork.Content.Services.Abstractions.Services;
using SocialNetwork.Content.Services.Services;
using SocialNetwork.EventBus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Database
var contentConnection = builder.Configuration.GetConnectionString("ContentDb")
    ?? throw new InvalidOperationException("ConnectionStrings:ContentDb is missing.");
builder.Services.AddDbContext<ContentDbContext>(options => options.UseSqlServer(contentConnection));

// Content application services and Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddScoped<IUserEventsService, UserEventsService>();

// RabbitMQ connection factory. EventBus owns and reuses the actual connection.
var rabbit = builder.Configuration.GetSection("RabbitMq");
var hostName = rabbit["HostName"] ?? "localhost";
var userName = rabbit["UserName"] ?? "guest";
var password = rabbit["Password"] ?? "guest";
var virtualHost = rabbit["VirtualHost"] ?? "/";
var port = int.TryParse(rabbit["Port"], out var configuredPort) ? configuredPort : 5672;

builder.Services.AddSingleton<IConnectionFactory>(_ => new ConnectionFactory
{
    HostName = hostName,
    Port = port,
    UserName = userName,
    Password = password,
    VirtualHost = virtualHost,
    ClientProvidedName = "SocialNetwork.Content",
    AutomaticRecoveryEnabled = true,
    TopologyRecoveryEnabled = true,
});

// Outbound events: Content writes to its SQL Outbox in the same transaction as domain changes.
builder.Services.AddOutboxPublisher<OutboxStore>(ContentEventTopology.Exchange);

// Inbound events: Auth publishes user lifecycle events to the shared integration exchange.
builder.Services.AddConsumer<UserCreatedConsumer>();
builder.Services.AddConsumer<UserUpdatedConsumer>();
builder.Services.AddConsumer<UserDeletedConsumer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();

// Required by WebApplicationFactory-based integration tests.
public partial class Program { }
