using OrdoApi.Utils;

namespace OrdoApi.Models;

public class Game
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string RedisKey => GetRedisKey(Id);
    public static string GetRedisKey(string id) => $"game:{id}";
    public string Language { get; set; } = "swedish"; 
    public GameStatus Status { get; set; } = GameStatus.WaitingForPlayers;
    public Board Board { get; set; } = new();
    public List<Tile> TileBag { get; set; } = TileValues.GetSwedishTileBag();
    public List<GuestPlayer> Players { get; set; } = [];
    public int CurrentTurnIndex { get; set; } // this needs to be public for JSON serialization
    public int ConsecutivePasses { get; set; }
    public string? CurrentPlayerId => Players.Count > 0 ? Players[CurrentTurnIndex].Id : null;

    public void StartGame()
    {
        if (Players.Count != 2)
        {
            throw new InvalidOperationException("A game requires exactly 2 players to start.");
        }

        Status = GameStatus.InProgress;

        ShuffleBag();
        
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
    
    public void ShuffleBag()
    {
        var random = new Random();
        TileBag = TileBag.OrderBy(_ => random.Next()).ToList();
    }

    public void AdvanceTurn()
    {
        if (Players.Count == 0) return;

        CurrentTurnIndex = (CurrentTurnIndex + 1) % Players.Count;
    }

    public string? GetWinnerId()
    {
        if (Players.Count == 0) return null;
        var maxScore = Players.Max(p => p.Score);
        var topPlayers = Players.Where(p => p.Score == maxScore).ToList();
        return topPlayers.Count == 1 ? topPlayers[0].Id : null; // if there's a tie, return null
    }

    public bool PassTurnAndCheck()
    {
        ConsecutivePasses++;
        AdvanceTurn();
        if (ConsecutivePasses < 3) return false; 
        Status = GameStatus.Completed;
        return true;
    }
}