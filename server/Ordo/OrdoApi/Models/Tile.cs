namespace OrdoApi.Models;

public class Tile
{
    public string Id { get; set; } = Guid.NewGuid().ToString(); // for frontend
    public char Letter { get; set; }
    public int Value { get; set; }
    public bool IsBlank { get; set; }
}