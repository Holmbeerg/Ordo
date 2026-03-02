using OrdoApi.Interfaces;
using OrdoApi.Models;
using OrdoApi.Utils;

namespace OrdoApi.Services;

internal record FormedWord(string Text, List<TilePlacement> Placements);

public class GameLogicService(IWordDictionaryService dictionaryService) : IGameLogicService
{
    public bool ValidateMove(Game game, GuestPlayer player, List<TilePlacement> placements)
    {
        if (placements.Count == 0) return false;

        if (!IsPlayerStateValid(game, player, placements)) return false;

        foreach (var p in placements)
        {
            if (p.Row < 0 || p.Row > 14 || p.Col < 0 || p.Col > 14) return false; // Out of bounds
            if (!game.Board.Squares[p.Row][p.Col].IsEmpty) return false; // Square already occupied
        }

        var isHorizontal = placements.All(p => p.Row == placements[0].Row);
        var isVertical = placements.All(p => p.Col == placements[0].Col);

        if (!isHorizontal && !isVertical && placements.Count > 1) return false;

        var isFirstTurn = game.Board.Squares[7][7].IsEmpty;

        if (isFirstTurn)
        {
            if (!IsFirstTurnValid(placements)) return false;
        }
        else
        {
            if (!IsConnectedToExistingTiles(game, placements)) return false;
        }

        if (placements.Count > 1 && !HasNoGaps(game, placements, isHorizontal))
        {
            return false;
        }

        var extractedWords = ExtractAllNewWords(game, placements);

        return extractedWords.All(word => dictionaryService.IsValidWord(word.Text, game.Language));
    }

    private static bool IsFirstTurnValid(List<TilePlacement> placements)
    {
        return placements.Any(p => p is { Row: 7, Col: 7 });
    }

    private static bool IsConnectedToExistingTiles(Game game, List<TilePlacement> placements)
    {
        foreach (var p in placements)
        {
            // Check Up, Down, Left, and Right for adjacent tiles
            if ((p.Row > 0 && !game.Board.Squares[p.Row - 1][p.Col].IsEmpty) ||
                (p.Row < 14 && !game.Board.Squares[p.Row + 1][p.Col].IsEmpty) ||
                (p.Col > 0 && !game.Board.Squares[p.Row][p.Col - 1].IsEmpty) ||
                (p.Col < 14 && !game.Board.Squares[p.Row][p.Col + 1].IsEmpty))
            {
                return true; // we only need one adjacent tile to be valid, so we can return true immediately
            }
        }

        return false;
    }

    private static bool HasNoGaps(Game game, List<TilePlacement> placements, bool isHorizontal)
    {
        if (isHorizontal)
        {
            var row = placements[0].Row;
            var minCol = placements.Min(p => p.Col);
            var maxCol = placements.Max(p => p.Col);

            for (var c = minCol; c <= maxCol; c++)
            {
                var hasTileBeingPlaced = placements.Any(p => p.Col == c);
                var hasExistingTile = !game.Board.Squares[row][c].IsEmpty;

                if (!hasTileBeingPlaced && !hasExistingTile) return false; // Found a gap
            }
        }
        else // isVertical
        {
            var col = placements[0].Col;
            var minRow = placements.Min(p => p.Row);
            var maxRow = placements.Max(p => p.Row);

            for (var r = minRow; r <= maxRow; r++)
            {
                var hasTileBeingPlaced = placements.Any(p => p.Row == r);
                var hasExistingTile = !game.Board.Squares[r][col].IsEmpty;

                if (!hasTileBeingPlaced && !hasExistingTile) return false; // Found a gap
            }
        }

        return true;
    }

    private static bool IsPlayerStateValid(Game game, GuestPlayer player, List<TilePlacement> placements)
    {
        if (game.CurrentPlayerId != player.Id) return false;
        
        var availableRack = player.Rack.ToList();

        foreach (var placement in placements)
        {
            Tile? matchingTile;
            // The client says this is a blank tile: find an actual blank in the rack
            matchingTile = placement.Tile.IsBlank ? availableRack.FirstOrDefault(t => t.IsBlank) :
                // Normal tile
                availableRack.FirstOrDefault(t => t.Letter == placement.Tile.Letter && !t.IsBlank);

            if (matchingTile == null) return false;
            availableRack.Remove(matchingTile);
        }

        return true;
    }

    private static FormedWord GetFullWord(Game game, List<TilePlacement> newPlacements, int row, int col,
        bool isHorizontal)
    {
        var wordPlacements = new List<TilePlacement>();

        if (isHorizontal)
        {
            var startCol = col;
            // Walk Left
            while (GetTileAt(game, newPlacements, row, startCol - 1) != null) startCol--;

            // Walk Right and collect
            var currentCol = startCol;
            while (true)
            {
                var tile = GetTileAt(game, newPlacements, row, currentCol);
                if (tile == null) break;

                wordPlacements.Add(new TilePlacement(row, currentCol, tile));
                currentCol++;
            }
        }
        else // isVertical
        {
            var startRow = row;
            // Walk Up
            while (GetTileAt(game, newPlacements, startRow - 1, col) != null) startRow--;

            // Walk Down and collect
            var currentRow = startRow;
            while (true)
            {
                var tile = GetTileAt(game, newPlacements, currentRow, col);
                if (tile == null) break;

                wordPlacements.Add(new TilePlacement(currentRow, col, tile));
                currentRow++;
            }
        }

        var text = new string(wordPlacements.Select(p => p.Tile.Letter).ToArray());
        return new FormedWord(text, wordPlacements);
    }

    private static List<FormedWord> ExtractAllNewWords(Game game, List<TilePlacement> placements)
    {
        var newWords = new List<FormedWord>();
        if (placements.Count == 0) return newWords;

        // Edge case: They only placed 1 tile
        if (placements.Count == 1)
        {
            var p = placements[0];
            var hWord = GetFullWord(game, placements, p.Row, p.Col, isHorizontal: true);
            var vWord = GetFullWord(game, placements, p.Row, p.Col, isHorizontal: false);

            if (hWord.Text.Length > 1) newWords.Add(hWord);
            if (vWord.Text.Length > 1) newWords.Add(vWord);

            return newWords;
        }

        // Standard case: They placed multiple tiles, so there is a clear "main" axis
        var isMainHorizontal = placements[0].Row == placements[1].Row;

        var mainWord = GetFullWord(game, placements, placements[0].Row, placements[0].Col, isMainHorizontal);
        if (mainWord.Text.Length > 1) newWords.Add(mainWord);

        // Grab any "cross words" formed by the individual tiles touching existing board tiles
        foreach (var p in placements)
        {
            var crossWord = GetFullWord(game, placements, p.Row, p.Col, !isMainHorizontal);
            if (crossWord.Text.Length > 1) newWords.Add(crossWord);
        }

        return newWords;
    }

    private static Tile? GetTileAt(Game game, List<TilePlacement> placements, int row, int col)
    {
        if (row < 0 || row >= Board.Size || col < 0 || col >= Board.Size) return null;

        var placement = placements.FirstOrDefault(p => p.Row == row && p.Col == col);
        if (placement != null) return placement.Tile;

        return game.Board.Squares[row][col].Tile;
    }
    
    public void SwapTiles(Game game, GuestPlayer player, List<Tile> tilesToSwap)
    {
        // Scrabble rule says you can only swap if there are at least 7 tiles left in the bag
        if (game.TileBag.Count < 7)
        {
            throw new InvalidOperationException("You cannot swap tiles when there are fewer than 7 tiles left in the bag.");
        }

        foreach (var tileToSwap in tilesToSwap)
        {
            var tileInRack = player.Rack.FirstOrDefault(t => t.Letter == tileToSwap.Letter);
            if (tileInRack == null)
            {
                throw new InvalidOperationException($"You do not have the letter {tileToSwap.Letter} in your rack.");
            }
            player.Rack.Remove(tileInRack);
        
            // Add it back to the bag
            game.TileBag.Add(tileInRack);
        }

        game.ShuffleBag();
        game.DrawTiles(player, tilesToSwap.Count);
        game.AdvanceTurn();
    }

    public int CalculateScore(Game game, List<TilePlacement> placements)
    {
        var extractedWords = ExtractAllNewWords(game, placements);
        var totalTurnScore = 0;

        foreach (var word in extractedWords)
        {
            var wordScore = 0;
            var wordMultiplier = 1;

            foreach (var wp in word.Placements)
            {
                // Always use server-side value, never trust the client-supplied Value
                var letterScore = wp.Tile.IsBlank ? 0 : TileValues.GetLetterValue(wp.Tile.Letter);

                // multipliers only apply to the newly placed tiles, not existing tiles that are part of the word
                var isNewPlacement = placements.Any(p => p.Row == wp.Row && p.Col == wp.Col);

                if (isNewPlacement)
                {
                    var square = game.Board.Squares[wp.Row][wp.Col];

                    switch (square.Multiplier)
                    {
                        case MultiplierType.DoubleLetter:
                            letterScore *= 2;
                            break;
                        case MultiplierType.TripleLetter:
                            letterScore *= 3;
                            break;
                        case MultiplierType.DoubleWord:
                            wordMultiplier *= 2;
                            break;
                        case MultiplierType.TripleWord:
                            wordMultiplier *= 3;
                            break;
                    }
                }

                wordScore += letterScore;
            }

            totalTurnScore += (wordScore * wordMultiplier);
        }

        if (placements.Count == 7)
        {
            totalTurnScore += 50;
        }

        return totalTurnScore;
    }

    public void ExecuteMove(Game game, GuestPlayer player, List<TilePlacement> placements)
    {
        var moveScore = CalculateScore(game, placements);
        player.Score += moveScore;

        foreach (var placement in placements)
        {
            game.Board.Squares[placement.Row][placement.Col].Tile = placement.Tile;

            // Match the rack tile by IsBlank for blanks, or by letter for normal tiles
            var rackTile = placement.Tile.IsBlank
                ? player.Rack.FirstOrDefault(t => t.IsBlank)
                : player.Rack.FirstOrDefault(t => t.Letter == placement.Tile.Letter && !t.IsBlank);

            if (rackTile != null)
            {
                player.Rack.Remove(rackTile);
            }
        }

        game.DrawTiles(player, placements.Count);

        if (game.TileBag.Count == 0 && player.Rack.Count == 0)
        {
            game.Status = GameStatus.Completed;
            return;
        }

        game.AdvanceTurn();
    }
}