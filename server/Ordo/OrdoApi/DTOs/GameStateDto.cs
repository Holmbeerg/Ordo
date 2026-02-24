namespace OrdoApi.Models;

public record GameStateDto(
    string GameId,
    string Status,
    string? CurrentTurnPlayerId,
    Board Board,
    
    string MyPlayerId,
    List<Tile> MyRack,
    int MyScore,
    
    string? OpponentId,
    string? OpponentName,
    int OpponentScore,
    int OpponentRackCount,
    
    int TilesRemainingInBag
);