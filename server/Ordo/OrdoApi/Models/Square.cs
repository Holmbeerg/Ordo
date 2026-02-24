namespace OrdoApi.Models;

public class Square
{
    public MultiplierType Multiplier { get; set; } = MultiplierType.None;
    public Tile? Tile { get; set; }
    
    public bool IsEmpty => Tile == null; // read-only expression-bodied property
}