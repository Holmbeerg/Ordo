using OrdoApi.Utils;

namespace OrdoApi.Models;

public class Game
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public GameStatus Status { get; set; } = GameStatus.WaitingForPlayers;
    
    public Board Board { get; } = new Board();
    public List<Tile> TileBag { get; private set; } = TileValues.GetSwedishTileBag();
    public List<GuestPlayer> Players { get; set; } = [];

    private int CurrentTurnIndex { get; set; } = 0;

    public GuestPlayer? GetCurrentPlayer()
    {
        return Players.Count == 0 ? null : Players[CurrentTurnIndex];
    }
    
    public void StartGame()
    {
        if (Players.Count != 2)
        {
            throw new InvalidOperationException("A game requires exactly 2 players to start.");
        }

        Status = GameStatus.InProgress;

        var random = new Random();
        TileBag = TileBag.OrderBy(_ => random.Next()).ToList();

        foreach (var player in Players)
        {
            DrawTiles(player, 7);
        }
    }

    public void DrawTiles(GuestPlayer player, int count)
    {
        var tilesNeeded = Math.Min(count, 7 - player.Rack.Count);

        for (var i = 0; i < tilesNeeded; i++)
        {
            if (TileBag.Count == 0) break;

            var tile = TileBag[0];
            TileBag.RemoveAt(0);
            
            player.Rack.Add(tile);
        }
    }
}
