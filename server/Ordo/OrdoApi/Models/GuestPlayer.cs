namespace OrdoApi.Models;

// https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/properties
public class GuestPlayer
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ConnectionId { get; set; } = string.Empty;
    public bool IsOnline { get; set; } = true;
    public string? CurrentGamedId { get; set; }
    public DateTime LastConnected { get; set; } 
    
    public string RedisKey => $"guest_player:{Id}"; // computed property
    
    public override string ToString()
    {
        return $"GuestPlayer {{ Id: {Id}, ConnectionId: {ConnectionId}, CurrentGamedId: {CurrentGamedId}, LastConnected: {LastConnected} }}";
    }
}