using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using OrdoApi.Interfaces;
using OrdoApi.Models;
using OrdoApi.Services;
using StackExchange.Redis;

namespace OrdoApi.Hubs;

public static class GameEvents
{
    public const string ReceiveGameState = "ReceiveGameState";
    public const string MatchFound = "MatchFound";
    public const string WaitingForMatch = "WaitingForMatch";
    public const string PlayerConnected = "PlayerConnected";
    public const string ReceiveError = "ReceiveError";
    public const string GameOver = "GameOver";
    public const string OpponentDisconnected = "OpponentDisconnected";
    public const string OpponentReconnected = "OpponentReconnected";
}

// https://learn.microsoft.com/en-us/aspnet/core/signalr/hubs?view=aspnetcore-9.0
// TODO: look into refactor using groups
public class GameHub(IConnectionMultiplexer redis, ILogger<GameHub> logger, IGameLogicService gameLogic) : Hub
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

                    // Notify the opponent in any active game
                    await NotifyOpponentConnectionChange(player, connected: false);
                }
            }

            await _db.KeyDeleteAsync($"connection:{Context.ConnectionId}");
        }

        if (exception != null)
            logger.LogError(exception, "A client disconnected with an error. ConnectionId: {ConnectionId}", Context.ConnectionId);
        else
            logger.LogInformation("A client disconnected gracefully. ConnectionId: {ConnectionId}", Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }

    private async Task ReconnectPlayer(string playerId)
    {
        var playerKey = $"guest_player:{playerId}";
        var playerJson = await _db.StringGetAsync(playerKey);

        if (playerJson.IsNullOrEmpty)
        {
            logger.LogWarning(
                "Reconnect failed: Player data not found in Redis for PlayerId: {PlayerId}. Creating new player",
                playerId);
            await CreateNewGuestPlayer();
            return;
        }

        var player = JsonSerializer.Deserialize<GuestPlayer>(playerJson!);
        if (player == null)
        {
            logger.LogWarning(
                "Reconnect failed: Could not deserialize player data for PlayerId: {PlayerId}. Creating new player",
                playerId);
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

        logger.LogInformation("Player {PlayerId} reconnected successfully with new ConnectionId: {ConnectionId}",
            player.Id, Context.ConnectionId);
        await Clients.Caller.SendAsync(GameEvents.PlayerConnected, player.Id);

        // Notify opponent that this player is back online
        await NotifyOpponentConnectionChange(player, connected: true);

        // TODO: add back to group if in a game etc
    }

    private async Task CreateNewGuestPlayer()
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

    private async Task NotifyOpponentConnectionChange(GuestPlayer player, bool connected)
    {
        if (player.CurrentGameId == null) return;

        var gameJson = await _db.StringGetAsync($"game:{player.CurrentGameId}");
        if (gameJson.IsNullOrEmpty) return;

        var game = JsonSerializer.Deserialize<Game>(gameJson!);
        if (game == null) return;

        var opponent = game.Players.FirstOrDefault(p => p.Id != player.Id);
        if (opponent == null || string.IsNullOrEmpty(opponent.ConnectionId)) return;

        var eventName = connected ? GameEvents.OpponentReconnected : GameEvents.OpponentDisconnected;
        await Clients.Client(opponent.ConnectionId).SendAsync(eventName);

        logger.LogInformation("Sent {Event} to opponent {OpponentId} in game {GameId}", eventName, opponent.Id, player.CurrentGameId);
    }

    private async Task<(Game game, string playerIdStr)?> GetValidatedGameContext(string gameId)
    {
        var playerIdStr = await _db.StringGetAsync($"connection:{Context.ConnectionId}");
        if (playerIdStr.IsNullOrEmpty) return null;

        var gameJson = await _db.StringGetAsync($"game:{gameId}");
        if (gameJson.IsNullOrEmpty) return null;

        var game = JsonSerializer.Deserialize<Game>(gameJson!);
        if (game == null) return null;

        return (game, playerIdStr.ToString());
    }

    private async Task<bool> ValidatePlayerTurn(Game game, string playerIdStr)
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
    
    private async Task SaveAndBroadcastGameState(Game game)
    {
        await _db.StringSetAsync($"game:{game.Id}", JsonSerializer.Serialize(game));

        foreach (var player in game.Players)
        {
            if (string.IsNullOrEmpty(player.ConnectionId)) continue;
            var dto = CreateGameStateDto(game, player.Id);
            await Clients.Client(player.ConnectionId).SendAsync(GameEvents.ReceiveGameState, dto);
        }
    }

    public async Task JoinMatchmaking(TimeControl timeControl, string playerId)
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

            // Store current game on each player so we can look it up from connection events
            opponent!.CurrentGameId = newGame.Id;
            player!.CurrentGameId = newGame.Id;
            await _db.StringSetAsync($"guest_player:{opponent.Id}", JsonSerializer.Serialize(opponent));
            await _db.StringSetAsync($"guest_player:{player.Id}", JsonSerializer.Serialize(player));

            await _db.StringSetAsync($"game:{newGame.Id}", JsonSerializer.Serialize(newGame));

            var playerDto = CreateGameStateDto(newGame, player.Id);
            var opponentDto = CreateGameStateDto(newGame, opponent.Id);

            await Clients.Caller.SendAsync(GameEvents.MatchFound, playerDto);

            var opponentConnectionId = opponent.ConnectionId;
            if (!string.IsNullOrEmpty(opponentConnectionId))
            {
                await Clients.Client(opponentConnectionId).SendAsync(GameEvents.MatchFound, opponentDto);
            }
        }
    }

    public async Task LeaveMatchmaking(TimeControl timeControl, string playerId)
    {
        var queueKey = $"matchmaking:{timeControl.ToString().ToLower()}";

        var removed = await _db.ListRemoveAsync(queueKey, playerId);

        if (removed > 0)
            logger.LogInformation("Player {PlayerId} left the {TimeControl} matchmaking queue.", playerId, timeControl);
        else
            logger.LogWarning("Player {PlayerId} tried to leave {TimeControl} queue but was not found.", playerId, timeControl);
    }

    public async Task<GameStateDto?> GetGameState(string gameId)
    {
        var playerId = await _db.StringGetAsync($"connection:{Context.ConnectionId}");
        if (playerId.IsNullOrEmpty) return null;

        var gameJson = await _db.StringGetAsync($"game:{gameId}");
        if (gameJson.IsNullOrEmpty) return null;

        var game = JsonSerializer.Deserialize<Game>(gameJson!);
        return game == null ? null : CreateGameStateDto(game, playerId.ToString());
    }

    public async Task SubmitMove(string gameId, List<TilePlacement> placements)
    {
        var context = await GetValidatedGameContext(gameId);
        if (context == null) return;

        var (game, playerIdStr) = context.Value;
        if (!await ValidatePlayerTurn(game, playerIdStr)) return;

        var player = game.Players.First(p => p.Id == playerIdStr);

        try
        {
            if (!gameLogic.ValidateMove(game, player, placements))
            {
                throw new InvalidOperationException("Invalid move.");
            }

            gameLogic.ExecuteMove(game, player, placements);

            await SaveAndBroadcastGameState(game);

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

    public async Task ResignGame(string gameId)
    {
        var context = await GetValidatedGameContext(gameId);
        if (context == null) return;

        var (game, playerIdStr) = context.Value;

        if (game.Status != GameStatus.InProgress)
        {
            await Clients.Caller.SendAsync(GameEvents.ReceiveError, "The game is not currently active.");
            return;
        }

        game.Status = GameStatus.Completed;

        await _db.StringSetAsync($"game:{game.Id}", JsonSerializer.Serialize(game));

        var opponent = game.Players.FirstOrDefault(p => p.Id != playerIdStr);

        logger.LogInformation("Player {PlayerId} resigned from game {GameId}", playerIdStr, gameId);

        await Clients.Caller.SendAsync(GameEvents.GameOver, opponent?.Id);

        if (opponent != null && !string.IsNullOrEmpty(opponent.ConnectionId))
        {
            await Clients.Client(opponent.ConnectionId).SendAsync(GameEvents.GameOver, opponent.Id);
        }
    }

    public async Task PassTurn(string gameId)
    {
        var context = await GetValidatedGameContext(gameId);
        if (context == null) return;

        var (game, playerIdStr) = context.Value;
        if (!await ValidatePlayerTurn(game, playerIdStr)) return;

        game.AdvanceTurn();

        await SaveAndBroadcastGameState(game);

        logger.LogInformation("Player {PlayerId} passed their turn in game {GameId}", playerIdStr, gameId);
    }

    public async Task ExchangeTiles(string gameId, List<Tile> tilesToSwap)
    {
        var context = await GetValidatedGameContext(gameId);
        if (context == null) return;

        var (game, playerIdStr) = context.Value;
        if (!await ValidatePlayerTurn(game, playerIdStr)) return;

        var player = game.Players.First(p => p.Id == playerIdStr);

        try
        {
            gameLogic.SwapTiles(game, player, tilesToSwap);

            await SaveAndBroadcastGameState(game);

            logger.LogInformation("Player {PlayerId} swapped {Count} tiles in game {GameId}", player.Id,
                tilesToSwap.Count, gameId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Invalid tile swap attempted by {PlayerId}", player.Id);
            await Clients.Caller.SendAsync(GameEvents.ReceiveError, ex.Message);
        }
    }
    
    private static GameStateDto CreateGameStateDto(Game game, string targetPlayerId)
    {
        var requestingPlayer = game.Players.FirstOrDefault(p => p.Id == targetPlayerId);
        var opponentPlayer = game.Players.FirstOrDefault(p => p.Id != targetPlayerId);

        if (requestingPlayer == null) throw new Exception("Player not found in game.");

        var boardDto = Enumerable.Range(0, Board.Size)
            .Select(row => Enumerable.Range(0, Board.Size)
                .Select(col =>
                {
                    var sq = game.Board.Squares[row][col];
                    return new SquareDto(sq.Multiplier.ToString(), sq.Tile);
                })
                .ToArray())
            .ToArray();

        return new GameStateDto(
            game.Id,
            game.Status.ToString(),
            game.CurrentPlayerId,
            boardDto,
            requestingPlayer.Id,
            requestingPlayer.Rack,
            requestingPlayer.Score,
            opponentPlayer?.Id,
            opponentPlayer?.Name,
            opponentPlayer?.Score ?? 0,
            opponentPlayer?.Rack.Count ?? 0,
            opponentPlayer?.IsOnline ?? false,
            game.TileBag.Count
        );
    }
}