using OrdoApi.Models;

namespace OrdoApi.Services;

public interface IMatchmakingResult;

public record QueuedResult : IMatchmakingResult;

public record MatchFoundResult(
    string Player2ConnectionId,
    GameStateDto Player1State,
    GameStateDto Player2State
) : IMatchmakingResult;

public interface IMatchmakingService
{
    Task<IMatchmakingResult> JoinQueueAsync(string playerId, TimeControl timeControl);
    Task LeaveQueueAsync(string playerId, TimeControl timeControl);
}