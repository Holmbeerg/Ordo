using OrdoApi.Models;
using Xunit;

namespace OrdoApi.Tests.Models;

public class BoardTests
{
    [Fact]
    public void Constructor_ShouldInitialize15x15Grid()
    {
        // Act
        var board = new Board();

        // Assert
        Assert.Equal(15, board.Squares.Length); // Checks Row count
        Assert.Equal(15, board.Squares[0].Length); // Checks Column count
    }

    [Fact]
    public void Constructor_ShouldSetCenterSquareToDoubleWord()
    {
        // Act
        var board = new Board();

        // Assert
        // [7, 7] is the center of a 0-indexed 15x15 board
        Assert.Equal(MultiplierType.DoubleWord, board.Squares[7][7].Multiplier);
    }

    [Theory]
    [InlineData(0, 0, MultiplierType.TripleWord)]    // Top Left
    [InlineData(0, 14, MultiplierType.TripleWord)]   // Top Right
    [InlineData(14, 0, MultiplierType.TripleWord)]   // Bottom Left
    [InlineData(14, 14, MultiplierType.TripleWord)]  // Bottom Right
    public void Constructor_ShouldSetCornersToTripleWord(int row, int col, MultiplierType expected)
    {
        // Act
        var board = new Board();

        // Assert
        Assert.Equal(expected, board.Squares[row][col].Multiplier);
    }

    [Fact]
    public void Constructor_ShouldLeaveStandardSquaresAsNone()
    {
        // Act
        var board = new Board();

        // Assert
        // [0, 1] is just a normal empty square next to the top-left corner
        Assert.Equal(MultiplierType.None, board.Squares[0][1].Multiplier);
    }
}