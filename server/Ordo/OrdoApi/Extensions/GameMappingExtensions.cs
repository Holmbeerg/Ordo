using OrdoApi.Models;

namespace OrdoApi.Extensions;

public static class GameMappingExtensions
{
    public static GameStateDto ToGameStateDto(this Game game, string targetPlayerId)
    {
        var requestingPlayer = game.Players.FirstOrDefault(p => p.Id == targetPlayerId);
        var opponentPlayer = game.Players.FirstOrDefault(p => p.Id != targetPlayerId);

        if (requestingPlayer == null) throw new Exception("Player not found in game.");

        var boardDto = Enumerable.Range(0, Board.Size)
            .Select(row => Enumerable.Range(0, Board.Size)
                .Select(col =>
                {
                    var sq = game.Board.Squares[row][col];
                    return new SquareDto(sq.Multiplier.ToString(), sq.Tile);
                })
                .ToArray())
            .ToArray();

        return new GameStateDto(
            game.Id,
            game.Status.ToString(),
            game.CurrentPlayerId,
            boardDto,
            requestingPlayer.Id,
            requestingPlayer.Rack,
            requestingPlayer.Score,
            opponentPlayer?.Id,
            opponentPlayer?.Name,
            opponentPlayer?.Score ?? 0,
            opponentPlayer?.Rack.Count ?? 0,
            opponentPlayer?.IsOnline ?? false,
            game.TileBag.Count
        );
    }
}