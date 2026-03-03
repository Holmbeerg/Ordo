using OrdoApi.Models; 

namespace OrdoApi.Constants; 

public static class RedisKeys
{
    public static readonly TimeSpan GameTtl = TimeSpan.FromHours(2);
    public static readonly TimeSpan GuestPlayerTtl = TimeSpan.FromHours(1);
    public static readonly TimeSpan ConnectionTtl = TimeSpan.FromHours(2);
    public static readonly TimeSpan MatchmakingQueueTtl = TimeSpan.FromMinutes(30);

    public static string GuestPlayer(string id) => $"guest_player:{id}";
    public static string Game(string id) => $"game:{id}";
    
    public static string Connection(string connectionId) => $"connection:{connectionId}";
    public static string MatchmakingQueue(TimeControl timeControl) => $"matchmaking:{timeControl.ToString().ToLower()}";
}