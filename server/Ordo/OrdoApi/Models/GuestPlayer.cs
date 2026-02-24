namespace OrdoApi.Models;

public class GuestPlayer(string? name = null) 
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.IsNullOrWhiteSpace(name) 
        ? $"Guest-{Random.Shared.Next(1000, 9999)}" 
        : name;

    public string ConnectionId { get; set; } = string.Empty;
    public bool IsOnline { get; set; } = true;
    public string? CurrentGameId { get; init; }
    public DateTime LastConnected { get; set; } = DateTime.UtcNow;
    
    public string RedisKey => $"guest_player:{Id}"; 
    
    public int Score { get; set; } // defaults to 0

    public List<Tile> Rack { get; set; } = [];

    public override string ToString()
    {
        return $"GuestPlayer {{ Id: {Id}, Name: {Name}, ConnectionId: {ConnectionId}, CurrentGameId: {CurrentGameId}, LastConnected: {LastConnected} }}";
    }
}