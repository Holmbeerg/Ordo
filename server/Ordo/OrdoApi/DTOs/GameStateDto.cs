namespace OrdoApi.Models;

public record SquareDto(string Multiplier, Tile? Tile);

public record GameStateDto(
    string GameId,
    string Status,
    string? CurrentTurnPlayerId,
    SquareDto[][] Board,

    string MyPlayerId,
    List<Tile> MyRack,
    int MyScore,

    string? OpponentId,
    string? OpponentName,
    int OpponentScore,
    int OpponentRackCount,
    bool OpponentIsConnected,

    int TilesRemainingInBag
);