using OrdoApi.Models;

namespace OrdoApi.Tests.Models;

public class GameTests
{
    [Fact]
    public void StartGame_ShouldThrowException_WhenPlayerCountIsNotTwo()
    {
        // Arrange
        var game = new Game();
        game.Players.Add(new GuestPlayer("Alice")); 

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => game.StartGame());
        Assert.Equal("A game requires exactly 2 players to start.", exception.Message);
    }

    [Fact]
    public void StartGame_ShouldDealSevenTilesToEachPlayer_AndSetStatus()
    {
        // Arrange
        var game = new Game();
        game.Players.Add(new GuestPlayer("Alice"));
        game.Players.Add(new GuestPlayer("Bob"));

        // Act
        game.StartGame();

        // Assert
        Assert.Equal(GameStatus.InProgress, game.Status);
        
        // Both players should have exactly 7 tiles
        Assert.Equal(7, game.Players[0].Rack.Count);
        Assert.Equal(7, game.Players[1].Rack.Count);
        
        // The bag started with 104 tiles, minus 14 dealt = 90 left
        Assert.Equal(90, game.TileBag.Count);
    }

    [Fact]
    public void DrawTiles_ShouldNeverExceedSevenTilesInRack()
    {
        // Arrange
        var game = new Game();
        var player = new GuestPlayer("Alice");
        
        // Manually give the player 3 tiles to simulate a partially full rack
        player.Rack.Add(game.TileBag[0]);
        player.Rack.Add(game.TileBag[1]);
        player.Rack.Add(game.TileBag[2]);
        game.TileBag.RemoveRange(0, 3);

        // Act
        // Attempt to draw 7 tiles, even though they already have 3
        game.DrawTiles(player, 7);

        // Assert
        Assert.Equal(7, player.Rack.Count); // Should only draw 4 to hit the cap of 7
    }
    
    [Fact]
    public void DrawTiles_ShouldStopDrawing_WhenBagIsEmptiedMidDraw()
    {
        // Arrange
        var game = new Game();
        var player = new GuestPlayer("Alice");
        
        // Artificially empty the bag down to exactly 2 tiles
        game.TileBag.RemoveRange(2, game.TileBag.Count - 2);

        // Act
        // Attempt to draw a full hand of 7, but only 2 exist in the bag
        game.DrawTiles(player, 7);

        // Assert
        Assert.Equal(2, player.Rack.Count); // The player should only get 2 tiles
        Assert.Empty(game.TileBag);         // The bag should now be completely empty
    }
}