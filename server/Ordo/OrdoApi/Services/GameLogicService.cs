using OrdoApi.Interfaces;
using OrdoApi.Models;

namespace OrdoApi.Services;

public class GameLogicService(IWordDictionaryService dictionaryService) : IGameLogicService
{
    public bool IsMoveValid(Game game, GuestPlayer player, List<TilePlacement> placements)
    {
        if (placements.Count == 0) return false;

        if (!IsPlayerStateValid(game, player, placements)) return false;

        foreach (var p in placements)
        {
            if (p.Row < 0 || p.Row > 14 || p.Col < 0 || p.Col > 14) return false; // Out of bounds
            if (!game.Board.Squares[p.Row, p.Col].IsEmpty) return false; // Square already occupied
        }

        var isHorizontal = placements.All(p => p.Row == placements[0].Row);
        var isVertical = placements.All(p => p.Col == placements[0].Col);

        if (!isHorizontal && !isVertical && placements.Count > 1) return false;

        var isFirstTurn = game.Board.Squares[7, 7].IsEmpty;

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
        
        return extractedWords.All(word => dictionaryService.IsValidWord(word, game.Language));
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
            if ((p.Row > 0 && !game.Board.Squares[p.Row - 1, p.Col].IsEmpty) ||
                (p.Row < 14 && !game.Board.Squares[p.Row + 1, p.Col].IsEmpty) ||
                (p.Col > 0 && !game.Board.Squares[p.Row, p.Col - 1].IsEmpty) ||
                (p.Col < 14 && !game.Board.Squares[p.Row, p.Col + 1].IsEmpty))
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
                var hasExistingTile = !game.Board.Squares[row, c].IsEmpty;

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
                var hasExistingTile = !game.Board.Squares[r, col].IsEmpty;

                if (!hasTileBeingPlaced && !hasExistingTile) return false; // Found a gap
            }
        }

        return true;
    }

    private static bool IsPlayerStateValid(Game game, GuestPlayer player, List<TilePlacement> placements)
    {
        if (game.CurrentPlayerId != player.Id) return false;

        var availableLetters = player.Rack.Select(t => t.Letter).ToList(); // Copy of player's rack letters

        return placements.All(placement => availableLetters.Remove(placement.Tile.Letter));
    }
    
    private static string GetFullWord(Game game, List<TilePlacement> placements, int row, int col, bool isHorizontal)
    {
        var letters = new List<char>();

        if (isHorizontal)
        {
            // Walk Left to find the very first letter of the word
            var startCol = col;
            while (GetTileAt(game, placements, row, startCol - 1) != null)
            {
                startCol--;
            }

            // walk Right, collecting letters until we hit an empty square
            var currentCol = startCol;
            while (true)
            {
                var tile = GetTileAt(game, placements, row, currentCol);
                if (tile == null) break; // End of the word
            
                letters.Add(tile.Letter);
                currentCol++;
            }
        }
        else // isVertical
        {
            // Walk Up to find the very first letter of the word
            var startRow = row;
            while (GetTileAt(game, placements, startRow - 1, col) != null)
            {
                startRow--;
            }

            // walk Down, collecting letters
            var currentRow = startRow;
            while (true)
            {
                var tile = GetTileAt(game, placements, currentRow, col);
                if (tile == null) break; // End of the word
            
                letters.Add(tile.Letter);
                currentRow++;
            }
        }

        return new string(letters.ToArray());
    }
    
    private static List<string> ExtractAllNewWords(Game game, List<TilePlacement> placements)
    {
        var newWords = new List<string>();
        if (placements.Count == 0) return newWords;

        // Edge case: They only placed 1 tile
        if (placements.Count == 1)
        {
            var p = placements[0];
            var hWord = GetFullWord(game, placements, p.Row, p.Col, isHorizontal: true);
            var vWord = GetFullWord(game, placements, p.Row, p.Col, isHorizontal: false);
        
            if (hWord.Length > 1) newWords.Add(hWord);
            if (vWord.Length > 1) newWords.Add(vWord);
        
            return newWords;
        }

        // Standard case: They placed multiple tiles, so there is a clear "main" axis
        var isMainHorizontal = placements[0].Row == placements[1].Row;
        
        var mainWord = GetFullWord(game, placements, placements[0].Row, placements[0].Col, isMainHorizontal);
        if (mainWord.Length > 1) newWords.Add(mainWord);

        // Grab any "cross words" formed by the individual tiles touching existing board tiles
        foreach (var p in placements)
        {
            var crossWord = GetFullWord(game, placements, p.Row, p.Col, !isMainHorizontal);
            if (crossWord.Length > 1) newWords.Add(crossWord);
        }

        return newWords;
    }

    private static Tile? GetTileAt(Game game, List<TilePlacement> placements, int row, int col)
    {
        if (row < 0 || row >= Board.Size || col < 0 || col >= Board.Size) return null;

        // First, check if the player is placing a tile exactly here right now
        var placement = placements.FirstOrDefault(p => p.Row == row && p.Col == col);
       
        // Otherwise, check what is actually sitting on the board
        return placement != null ? placement.Tile : game.Board.Squares[row, col].Tile;
    }
    
    public int CalculateScore(Game game, List<TilePlacement> placements)
    {
        throw new NotImplementedException("N/A");
    }

    public void ExecuteMove(Game game, GuestPlayer player, List<TilePlacement> placements)
    {
        throw new NotImplementedException("N/A");
    }
}