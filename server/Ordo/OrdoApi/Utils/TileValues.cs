using OrdoApi.Models;

namespace OrdoApi.Utils;

public static class TileValues
{
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