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
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()); // for TimeControl and GameStatus enums
    });
builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379")); // redis
builder.Services.AddSingleton<IWordDictionaryService, WordDictionaryService>();
builder.Services.AddSingleton<IGameLogicService, GameLogicService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // dev server
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