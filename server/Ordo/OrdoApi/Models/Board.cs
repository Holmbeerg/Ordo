namespace OrdoApi.Models;

public class Board
{
    // A jagged array representing the 15x15 grid (Square[][] is JSON-serializable, Square[,] is not)
    public Square[][] Squares { get; set; }
    public const int Size = 15;

    public Board()
    {
        Squares = new Square[Size][];
        InitializeEmptyBoard();
        ApplyMultipliers();
    }

    private void InitializeEmptyBoard()
    {
        for (var row = 0; row < Size; row++)
        {
            Squares[row] = new Square[Size];
            for (var col = 0; col < Size; col++)
            {
                Squares[row][col] = new Square();
            }
        }
    }

    private void ApplyMultipliers() // standard Scrabble board multipliers based on official layout
    {
        // Triple Word 
        SetMultipliers(MultiplierType.TripleWord, [
            (0, 0), (0, 7), (0, 14),
            (7, 0),         (7, 14),
            (14, 0), (14, 7), (14, 14)
        ]);

        // Double Word 
        SetMultipliers(MultiplierType.DoubleWord, [
            (1, 1), (2, 2), (3, 3), (4, 4),
            (10, 10), (11, 11), (12, 12), (13, 13),
            (1, 13), (2, 12), (3, 11), (4, 10),
            (10, 4), (11, 3), (12, 2), (13, 1),
            (7, 7) // Center starting square
        ]);

        // Triple Letter 
        SetMultipliers(MultiplierType.TripleLetter, [
            (1, 5), (1, 9), (5, 1), (5, 5), (5, 9), (5, 13),
            (9, 1), (9, 5), (9, 9), (9, 13), (13, 5), (13, 9)
        ]);

        // Double Letter
        SetMultipliers(MultiplierType.DoubleLetter, [
            (0, 3), (0, 11), (2, 6), (2, 8), (3, 0), (3, 7), (3, 14),
            (6, 2), (6, 6), (6, 8), (6, 12), (7, 3), (7, 11),
            (8, 2), (8, 6), (8, 8), (8, 12), (11, 0), (11, 7), (11, 14),
            (12, 6), (12, 8), (14, 3), (14, 11)
        ]);

    }

    // Helper method to keep the initialization clean
    private void SetMultipliers(MultiplierType type, (int row, int col)[] coordinates)
    {
        foreach (var (row, col) in coordinates)
        {
            Squares[row][col].Multiplier = type;
        }
    }
}