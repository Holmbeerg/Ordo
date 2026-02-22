namespace OrdoApi.Models;

public class Player(string name)
{
    public string Id { get; set; } = Guid.NewGuid().ToString(); 
    public string Name { get; set; } = name;
    public int Score { get; set; } = 0;
    
    // Maximum of 7 tiles in the rack at any times
    public List<Tile> Rack { get; set; } = [];
}