using OrdoApi.Utils;

namespace OrdoApi.Tests.Utils;

public class TileValuesTests
{
    [Fact]
    public void GetSwedishTileBag_ShouldReturnExactly104Tiles()
    {
        // Act
        var bag = TileValues.GetSwedishTileBag();

        // Assert
        Assert.Equal(104, bag.Count);
    }

    [Fact]
    public void GetSwedishTileBag_ShouldContainExactlyTwoBlanks()
    {
        // Act
        var bag = TileValues.GetSwedishTileBag();
        var blanks = bag.Where(t => t.IsBlank).ToList();

        // Assert
        Assert.Equal(2, blanks.Count);
        Assert.All(blanks, b => Assert.Equal(0, b.Value)); // Blanks must be worth 0 points
    }

    [Theory]
    [InlineData('A', 1, 9)]  
    [InlineData('C', 8, 1)]  
    [InlineData('Z', 8, 1)]  
    public void GetSwedishTileBag_ShouldHaveCorrectDistributionForSpecificLetters(char letter, int expectedValue, int expectedCount)
    {
        // Act
        var bag = TileValues.GetSwedishTileBag();
        var specificTiles = bag.Where(t => t.Letter == letter).ToList();

        // Assert
        Assert.Equal(expectedCount, specificTiles.Count);
        Assert.All(specificTiles, t => Assert.Equal(expectedValue, t.Value));
    }
}