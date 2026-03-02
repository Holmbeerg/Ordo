using OrdoApi.Models;

namespace OrdoApi.Utils;

public static class TileValues
{
    private static readonly Dictionary<char, int> SwedishLetterValues = new()
    {
        { 'A', 1 }, { 'B', 3 }, { 'C', 8 }, { 'D', 1 }, { 'E', 1 },
        { 'F', 3 }, { 'G', 2 }, { 'H', 3 }, { 'I', 1 }, { 'J', 7 },
        { 'K', 3 }, { 'L', 2 }, { 'M', 3 }, { 'N', 1 }, { 'O', 2 },
        { 'P', 4 }, { 'R', 1 }, { 'S', 1 }, { 'T', 1 }, { 'U', 4 },
        { 'V', 3 }, { 'X', 8 }, { 'Y', 7 }, { 'Z', 8 }, { 'Å', 4 },
        { 'Ä', 4 }, { 'Ö', 4 }, { ' ', 0 }
    };

    public static int GetLetterValue(char letter) =>
        SwedishLetterValues.GetValueOrDefault(char.ToUpper(letter), 0);

    public static List<Tile> GetSwedishTileBag()
    {
        var bag = new List<Tile>();

        // (Letter, Points, Count) https://wordfeud.com/wfweb/help/
        AddTiles('A', 1, 9); AddTiles('B', 3, 2); AddTiles('C', 8, 1);
        AddTiles('D', 1, 5); AddTiles('E', 1, 8); AddTiles('F', 3, 2);
        AddTiles('G', 2, 3); AddTiles('H', 3, 2); AddTiles('I', 1, 5);
        AddTiles('J', 7, 1); AddTiles('K', 3, 3); AddTiles('L', 2, 5);
        AddTiles('M', 3, 3); AddTiles('N', 1, 6); AddTiles('O', 2, 6);
        AddTiles('P', 4, 2); AddTiles('R', 1, 8); AddTiles('S', 1, 8);
        AddTiles('T', 1, 9); AddTiles('U', 4, 3); AddTiles('V', 3, 2);
        AddTiles('X', 8, 1); AddTiles('Y', 7, 1); AddTiles('Z', 8, 1);
        AddTiles('Å', 4, 2); AddTiles('Ä', 4, 2); AddTiles('Ö', 4, 2);
        
        // Add blank tiles
        bag.Add(new Tile { Letter = ' ', Value = 0, IsBlank = true });
        bag.Add(new Tile { Letter = ' ', Value = 0, IsBlank = true });

        return bag;
        
        void AddTiles(char letter, int value, int count)
        {
            for (var i = 0; i < count; i++)
            {
                bag.Add(new Tile { Letter = letter, Value = value });
            }
        }
    }
}