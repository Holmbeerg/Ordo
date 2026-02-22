namespace OrdoApi.Models;

public class GuestPlayer(string name) // primary constructor with name parameter
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; } = name;
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