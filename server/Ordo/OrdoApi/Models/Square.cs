namespace OrdoApi.Models;

public class Square
{
    public MultiplierType Multiplier { get; set; } = MultiplierType.None;
    public Tile? PlacedTile { get; set; }
    
    public bool IsEmpty => PlacedTile == null; // read-only expression-bodied property
}