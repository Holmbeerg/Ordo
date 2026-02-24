using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using OrdoApi.Models;
using OrdoApi.Services;
using StackExchange.Redis;

namespace OrdoApi.Hubs;

public static class GameEvents
{
    public const string GameStateUpdated = "GameStateUpdated";
    public const string MatchFound = "MatchFound";
    public const string WaitingForMatch = "WaitingForMatch";
    public const string PlayerConnected = "PlayerConnected";
    public const string ReceiveError = "ReceiveError";
    public const string GameOver = "GameOver";
}

// https://learn.microsoft.com/en-us/aspnet/core/signalr/hubs?view=aspnetcore-9.0
// TODO: look into refactor using groups
public class GameHub(IConnectionMultiplexer redis, ILogger<GameHub> logger, GameLogicService gameLogic) : Hub
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
            await ReconnectPlayerAsync(playerId);
        }
        else
        {
            logger.LogInformation("No PlayerId found in query. Creating new guest player.");
            await CreateNewGuestPlayerAsync();
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
            logger.LogError(exception, "A client disconnected with an error. ConnectionId: {ConnectionId}",
                Context.ConnectionId);
        }
        else
        {
            logger.LogInformation("A client disconnected gracefully. ConnectionId: {ConnectionId}",
                Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private async Task ReconnectPlayerAsync(string playerId)
    {
        var playerKey = $"guest_player:{playerId}";
        var playerJson = await _db.StringGetAsync(playerKey);

        if (playerJson.IsNullOrEmpty)
        {
            logger.LogWarning(
                "Reconnect failed: Player data not found in Redis for PlayerId: {PlayerId}. Creating new player",
                playerId);
            await CreateNewGuestPlayerAsync();
            return;
        }

        var player = JsonSerializer.Deserialize<GuestPlayer>(playerJson!);
        if (player == null)
        {
            logger.LogWarning(
                "Reconnect failed: Could not deserialize player data for PlayerId: {PlayerId}. Creating new player",
                playerId);
            await CreateNewGuestPlayerAsync();
            return;
        }

        player.ConnectionId = Context.ConnectionId;
        player.IsOnline = true;
        player.LastConnected = DateTime.UtcNow;

        var updatedPlayerJson = JsonSerializer.Serialize(player);
        await _db.StringSetAsync(playerKey, updatedPlayerJson);

        // we map connectionId to playerId for lookup on disconnect
        await _db.StringSetAsync($"connection:{Context.ConnectionId}", player.Id);

        logger.LogInformation("Player {PlayerId} reconnected successfully with new ConnectionId: {ConnectionId}",
            player.Id, Context.ConnectionId);
        await Clients.Caller.SendAsync(GameEvents.PlayerConnected, player.Id);

        // TODO: add back to group if in a game etc
    }

    private async Task CreateNewGuestPlayerAsync()
    {
        var randomNum = new Random().Next(1000, 9999);
        var guestName = $"Guest#{randomNum}";

        var player = new GuestPlayer(guestName)
        {
            ConnectionId = Context.ConnectionId
        };

        var playerJson = JsonSerializer.Serialize(player);
        await _db.StringSetAsync(player.RedisKey, playerJson);

        await _db.StringSetAsync($"connection:{Context.ConnectionId}", player.Id);

        logger.LogInformation("New guest player connected: {ConnectionId}, assigned PlayerId: {PlayerId}",
            Context.ConnectionId, player.Id);

        await Clients.Caller.SendAsync(GameEvents.PlayerConnected, player.Id);
    }

    private async Task<(Game game, string playerIdStr)?> GetValidatedGameContextAsync(string gameId)
    {
        var playerIdStr = await _db.StringGetAsync($"connection:{Context.ConnectionId}");
        if (playerIdStr.IsNullOrEmpty) return null;

        var gameJson = await _db.StringGetAsync($"game:{gameId}");
        if (gameJson.IsNullOrEmpty) return null;

        var game = JsonSerializer.Deserialize<Game>(gameJson!);
        if (game == null) return null;

        return (game, playerIdStr.ToString());
    }

    private async Task<bool> ValidatePlayerTurnAsync(Game game, string playerIdStr)
    {
        if (game.Status != GameStatus.InProgress)
        {
            await Clients.Caller.SendAsync(GameEvents.ReceiveError, "The game is not currently active.");
            return false;
        }

        if (game.CurrentPlayerId == playerIdStr) return true;

        await Clients.Caller.SendAsync(GameEvents.ReceiveError, "It is not your turn!");
        return false;
    }

    private async Task SaveAndBroadcastGameStateAsync(Game game, GuestPlayer currentPlayer)
    {
        await _db.StringSetAsync($"game:{game.Id}", JsonSerializer.Serialize(game));

        await Clients.Caller.SendAsync(GameEvents.GameStateUpdated);

        var opponent = game.Players.First(p => p.Id != currentPlayer.Id);
        if (!string.IsNullOrEmpty(opponent.ConnectionId))
        {
            await Clients.Client(opponent.ConnectionId).SendAsync(GameEvents.GameStateUpdated);
        }
    }

    public async Task JoinMatchmakingAsync(TimeControl timeControl, string playerId)
    {
        var queueKey =
            $"matchmaking:{timeControl.ToString().ToLower()}"; // adjust to language specific when we add that feature

        logger.LogInformation("Player {PlayerId} is searching for a {TimeControl} match.", playerId, timeControl);

        // Try to grab an opponent from the front of the line
        var opponentId = await _db.ListLeftPopAsync(queueKey);

        if (opponentId.IsNullOrEmpty)
        {
            await _db.ListRightPushAsync(queueKey, playerId);

            await Clients.Caller.SendAsync(
                GameEvents.WaitingForMatch); // tell the client to show a "waiting for match" message or spinner
            logger.LogInformation("Player {PlayerId} is waiting in queue: {QueueKey}", playerId, queueKey);
        }
        else if (opponentId == playerId)
        {
            // if the playerId is the same as the opponentId, it means they were the only one in the queue and got popped out by their own request. We need to put them back in.
            await _db.ListRightPushAsync(queueKey, playerId);
            await Clients.Caller.SendAsync(GameEvents.WaitingForMatch);
        }
        else
        {
            logger.LogInformation("Match found! {Player1} vs {Player2} for {TimeControl}", opponentId, playerId,
                timeControl);

            var opponentJson = await _db.StringGetAsync($"guest_player:{opponentId}");
            var playerJson = await _db.StringGetAsync($"guest_player:{playerId}");

            var opponent = JsonSerializer.Deserialize<GuestPlayer>(opponentJson!);
            var player = JsonSerializer.Deserialize<GuestPlayer>(playerJson!);

            var newGame = new Game();
            newGame.Players.Add(opponent!);
            newGame.Players.Add(player!);

            newGame.StartGame();

            await _db.StringSetAsync($"game:{newGame.Id}", JsonSerializer.Serialize(newGame));

            await Clients.Caller.SendAsync(GameEvents.MatchFound, newGame.Id);

            var opponentConnectionId = opponent!.ConnectionId;
            if (!string.IsNullOrEmpty(opponentConnectionId))
            {
                await Clients.Client(opponentConnectionId).SendAsync(GameEvents.MatchFound, newGame.Id);
            }
        }
    }

    public async Task<GameStateDto?> GetGameStateAsync(string gameId)
    {
        var playerId = await _db.StringGetAsync($"connection:{Context.ConnectionId}");
        if (playerId.IsNullOrEmpty) return null;

        var gameJson = await _db.StringGetAsync($"game:{gameId}");
        if (gameJson.IsNullOrEmpty) return null;

        var game = JsonSerializer.Deserialize<Game>(gameJson!);
        if (game == null) return null;

        var requestingPlayer = game.Players.FirstOrDefault(p => p.Id == playerId);
        var opponentPlayer = game.Players.FirstOrDefault(p => p.Id != playerId);

        if (requestingPlayer == null) return null;

        return new GameStateDto(
            game.Id,
            game.Status.ToString(),
            game.CurrentPlayerId,
            game.Board,
            requestingPlayer.Id,
            requestingPlayer.Rack,
            requestingPlayer.Score,
            opponentPlayer?.Id,
            opponentPlayer?.Name,
            opponentPlayer?.Score ?? 0,
            opponentPlayer?.Rack.Count ?? 0,
            game.TileBag.Count
        );
    }

    public async Task SubmitMove(string gameId, List<TilePlacement> placements)
    {
        var context = await GetValidatedGameContextAsync(gameId);
        if (context == null) return;

        var (game, playerIdStr) = context.Value;
        if (!await ValidatePlayerTurnAsync(game, playerIdStr)) return;

        var player = game.Players.First(p => p.Id == playerIdStr);

        try
        {
            gameLogic.ValidateMove(game, player, placements);
            gameLogic.ExecuteMove(game, player, placements);

            await SaveAndBroadcastGameStateAsync(game, player);

            if (game.Status == GameStatus.Completed)
            {
                logger.LogInformation("Game {GameId} has completed! Winner: {WinnerId}", game.Id, player.Id);
                var opponent = game.Players.First(p => p.Id != player.Id);
                await Clients.Caller.SendAsync(GameEvents.GameOver, player.Id);
                if (!string.IsNullOrEmpty(opponent.ConnectionId))
                {
                    await Clients.Client(opponent.ConnectionId).SendAsync(GameEvents.GameOver, player.Id);
                }
            }
        }
        catch (Exception ex)
        {
            // If ValidateMove throws an error (e.g., "Word not in dictionary"), we send it back to the client
            logger.LogWarning(ex, "Invalid move attempted by Player {PlayerId}", player.Id);
            await Clients.Caller.SendAsync(GameEvents.ReceiveError, ex.Message);
        }
    }

    public async Task PassTurn(string gameId)
    {
        var context = await GetValidatedGameContextAsync(gameId);
        if (context == null) return;

        var (game, playerIdStr) = context.Value;
        if (!await ValidatePlayerTurnAsync(game, playerIdStr)) return;

        var player = game.Players.First(p => p.Id == playerIdStr);

        game.AdvanceTurn();

        await SaveAndBroadcastGameStateAsync(game, player);

        logger.LogInformation("Player {PlayerId} passed their turn in game {GameId}", playerIdStr, gameId);
    }

    public async Task SwapTiles(string gameId, List<Tile> tilesToSwap)
    {
        var context = await GetValidatedGameContextAsync(gameId);
        if (context == null) return;

        var (game, playerIdStr) = context.Value;
        if (!await ValidatePlayerTurnAsync(game, playerIdStr)) return;

        var player = game.Players.First(p => p.Id == playerIdStr);

        try
        {
            gameLogic.SwapTiles(game, player, tilesToSwap);

            await SaveAndBroadcastGameStateAsync(game, player);

            logger.LogInformation("Player {PlayerId} swapped {Count} tiles in game {GameId}", player.Id,
                tilesToSwap.Count, gameId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Invalid tile swap attempted by {PlayerId}", player.Id);
            await Clients.Caller.SendAsync(GameEvents.ReceiveError, ex.Message);
        }
    }
}