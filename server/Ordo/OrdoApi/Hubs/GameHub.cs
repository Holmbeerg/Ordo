using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using OrdoApi.Models;
using StackExchange.Redis;

namespace OrdoApi.Hubs;

// https://learn.microsoft.com/en-us/aspnet/core/signalr/hubs?view=aspnetcore-9.0
public class GameHub(IConnectionMultiplexer redis, ILogger<GameHub> logger) : Hub
{
    private readonly IDatabase _db = redis.GetDatabase();

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext == null)
        {
            logger.LogError("HttpContext is null for ConnectionId: {ConnectionId}", Context.ConnectionId);
            return;
        }

        var playerId = httpContext.Request.Query["playerId"].ToString();

        if (!string.IsNullOrEmpty(playerId))
        {
            logger.LogInformation("Player with ID {PlayerId} is attempting to reconnect.", playerId);
            await ReconnectPlayer(playerId);
        }
        else
        {
            logger.LogInformation("No PlayerId found in query. Creating new guest player.");
            await CreateNewGuestPlayer();
        }

        await base.OnConnectedAsync();
    }
    
    // TODO: timer for player to reconnect etc
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // we find the player by connectionId to mark them offline
        var playerId = await _db.StringGetAsync($"connection:{Context.ConnectionId}");

        if (!playerId.IsNullOrEmpty)
        {
            var playerKey = $"guest_player:{playerId}";
            var playerJson = await _db.StringGetAsync(playerKey);

            if (!playerJson.IsNullOrEmpty)
            {
                var player = JsonSerializer.Deserialize<GuestPlayer>(playerJson!);
                if (player != null)
                {
                    player.IsOnline = false;
                    player.LastConnected = DateTime.UtcNow;

                    await _db.StringSetAsync(playerKey, JsonSerializer.Serialize(player));
                    logger.LogInformation("Player {PlayerId} disconnected. Status set to offline.", player.Id);
                    
                    // TODO: if in a game, notify other player etc
                }
            }
            // clean up the connection mapping
            await _db.KeyDeleteAsync($"connection:{Context.ConnectionId}");
        }

        if (exception != null)
        {
            logger.LogError(exception, "A client disconnected with an error. ConnectionId: {ConnectionId}", Context.ConnectionId);
        }
        else
        {
            logger.LogInformation("A client disconnected gracefully. ConnectionId: {ConnectionId}", Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }
    
    private async Task ReconnectPlayer(string playerId)
    {
        var playerKey = $"guest_player:{playerId}";
        var playerJson = await _db.StringGetAsync(playerKey);

        if (playerJson.IsNullOrEmpty)
        {
            logger.LogWarning("Reconnect failed: Player data not found in Redis for PlayerId: {PlayerId}. Creating new player", playerId);
            await CreateNewGuestPlayer();
            return;
        }
        var player = JsonSerializer.Deserialize<GuestPlayer>(playerJson!);
        if (player == null)
        {
            logger.LogWarning("Reconnect failed: Could not deserialize player data for PlayerId: {PlayerId}. Creating new player", playerId);
            await CreateNewGuestPlayer();
            return;
        }
        
        player.ConnectionId = Context.ConnectionId;
        player.IsOnline = true;
        player.LastConnected = DateTime.UtcNow;
        
        var updatedPlayerJson = JsonSerializer.Serialize(player);
        await _db.StringSetAsync(playerKey, updatedPlayerJson);
        
        // we map connectionId to playerId for lookup on disconnect
        await _db.StringSetAsync($"connection:{Context.ConnectionId}", player.Id);
        
        logger.LogInformation("Player {PlayerId} reconnected successfully with new ConnectionId: {ConnectionId}", player.Id, Context.ConnectionId);
        await Clients.Caller.SendAsync("PlayerConnected", player.Id);
        
        // TODO: add back to group if in a game etc
    }
    
    private async Task CreateNewGuestPlayer()
    {
        var player = new GuestPlayer
        {
            ConnectionId = Context.ConnectionId
        };
        var playerJson = JsonSerializer.Serialize(player);
        await _db.StringSetAsync(player.RedisKey, playerJson);
        
        await _db.StringSetAsync($"connection:{Context.ConnectionId}", player.Id);
        
        logger.LogInformation("New guest player connected: {ConnectionId}, assigned PlayerId: {PlayerId}",
            Context.ConnectionId, player.Id);
        await Clients.Caller.SendAsync("PlayerConnected", player.Id);
    }

    public async Task JoinMatchmaking(TimeControl timeControl, string playerId)
    {
        
    }
}