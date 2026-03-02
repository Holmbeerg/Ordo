namespace OrdoApi.DTOs;

public record TileDto(string Id, char Letter, int Value, bool IsBlank);

public record TileDtoPlacement(int Row, int Col, char Letter, bool IsBlank);

public record SquareDto(string Multiplier, TileDto? Tile);

public record GameStateDto(
    string GameId,
    string Status,
    string? CurrentTurnPlayerId,
    SquareDto[][] Board,

    string MyPlayerId,
    List<TileDto> MyRack,
    int MyScore,

    string? OpponentId,
    string? OpponentName,
    int OpponentScore,
    int OpponentRackCount,
    bool OpponentIsConnected,

    int TilesRemainingInBag
);