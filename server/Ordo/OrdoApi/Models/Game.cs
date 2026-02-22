using OrdoApi.Utils;

namespace OrdoApi.Models;

public class Game
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public GameStatus Status { get; set; } = GameStatus.WaitingForPlayers;
    
    public Board Board { get; } = new Board();
    public List<Tile> TileBag { get; private set; } = TileValues.GetSwedishTileBag();

    private List<GuestPlayer> Players { get; set; } = [];

    private int CurrentTurnIndex { get; set; } = 0;

    public GuestPlayer? GetCurrentPlayer()
    {
        return Players.Count == 0 ? null : Players[CurrentTurnIndex];
    }
}