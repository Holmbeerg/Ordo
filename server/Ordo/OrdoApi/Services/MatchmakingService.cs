using System.Text.Json;
using OrdoApi.Extensions;
using OrdoApi.Models;
using StackExchange.Redis;

namespace OrdoApi.Services;

public class MatchmakingService(IConnectionMultiplexer redis, ILogger<MatchmakingService> logger) : IMatchmakingService
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<IMatchmakingResult> JoinQueueAsync(string playerId, TimeControl timeControl)
    {
        var queueKey = $"matchmaking:{timeControl.ToString().ToLower()}";

        logger.LogInformation("Player {PlayerId} is searching for a {TimeControl} match.", playerId, timeControl);

        var opponentId = await _db.ListLeftPopAsync(queueKey);

        if (opponentId.IsNullOrEmpty || opponentId == playerId)
        {
            await _db.ListRightPushAsync(queueKey, playerId);
            logger.LogInformation("Player {PlayerId} is waiting in queue: {QueueKey}", playerId, queueKey);
            return new QueuedResult();
        }

        logger.LogInformation("Match found! {Player1} vs {Player2} for {TimeControl}", opponentId, playerId, timeControl);

        var opponentJson = await _db.StringGetAsync($"guest_player:{opponentId}");
        var playerJson = await _db.StringGetAsync($"guest_player:{playerId}");

        var opponent = JsonSerializer.Deserialize<GuestPlayer>((string)opponentJson!);
        var player = JsonSerializer.Deserialize<GuestPlayer>((string)playerJson!);

        if (opponent == null || player == null)
        {
            logger.LogError("Failed to deserialize player data during matchmaking. PlayerId: {PlayerId}, OpponentId: {OpponentId}", playerId, opponentId);
            await _db.ListRightPushAsync(queueKey, playerId);
            return new QueuedResult();
        }

        var newGame = new Game();
        newGame.Players.Add(opponent);
        newGame.Players.Add(player);
        newGame.StartGame();

        opponent.CurrentGameId = newGame.Id;
        player.CurrentGameId = newGame.Id;

        await _db.StringSetAsync($"guest_player:{opponent.Id}", JsonSerializer.Serialize(opponent));
        await _db.StringSetAsync($"guest_player:{player.Id}", JsonSerializer.Serialize(player));
        await _db.StringSetAsync($"game:{newGame.Id}", JsonSerializer.Serialize(newGame));

        var playerDto = newGame.ToGameStateDto(player.Id);
        var opponentDto = newGame.ToGameStateDto(opponent.Id);

        return new MatchFoundResult(
            opponent.ConnectionId ?? string.Empty,
            playerDto,
            opponentDto
        );
    }

    public async Task LeaveQueueAsync(string playerId, TimeControl timeControl)
    {
        var queueKey = $"matchmaking:{timeControl.ToString().ToLower()}";
        var removed = await _db.ListRemoveAsync(queueKey, playerId);

        if (removed > 0)
            logger.LogInformation("Player {PlayerId} left the {TimeControl} matchmaking queue.", playerId, timeControl);
        else
            logger.LogWarning("Player {PlayerId} tried to leave {TimeControl} queue but was not found.", playerId, timeControl);
    }
}