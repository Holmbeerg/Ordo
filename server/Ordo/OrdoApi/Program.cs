using System.Text.Json.Serialization;
using OrdoApi.Exceptions;
using OrdoApi.Hubs;
using OrdoApi.Interfaces;
using OrdoApi.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
var signalRBuilder = builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()); // for TimeControl and GameStatus enums
    });
var azureSignalRConnectionString = builder.Configuration["Azure:SignalR:ConnectionString"];
if (!string.IsNullOrEmpty(azureSignalRConnectionString))
{
    signalRBuilder.AddAzureSignalR(azureSignalRConnectionString);
}
builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var redisConnectionString = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
var redis = await ConnectionMultiplexer.ConnectAsync(redisConnectionString);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
builder.Services.AddSingleton<IWordDictionaryService, WordDictionaryService>();
builder.Services.AddSingleton<IGameLogicService, GameLogicService>();
builder.Services.AddSingleton<IMatchmakingService, MatchmakingService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://ordo-dek.pages.dev"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowVueApp");
app.MapControllers();

app.MapHub<GameHub>("/gameHub");

// probably not the best option
var wordDictionaryService = app.Services.GetRequiredService<IWordDictionaryService>();
await wordDictionaryService.InitializeAsync();

app.Logger.LogInformation("Starting Ordo backend...");

await app.RunAsync();