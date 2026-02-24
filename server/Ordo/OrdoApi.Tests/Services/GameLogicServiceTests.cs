using OrdoApi.Models;
using OrdoApi.Services;

namespace OrdoApi.Tests.Services;

public class GameLogicServiceTests
{
    private readonly GameLogicService _gameLogic = new(new FakeDictionaryService());

    [Fact]
    public void IsMoveValid_ShouldReturnFalse_WhenPlacementListIsEmpty()
    {
        var game = new Game();
        var player = new GuestPlayer("Olof");
        var placements = new List<TilePlacement>();

        var isValid = _gameLogic.ValidateMove(game, player, placements);

        Assert.False(isValid);
    }

    [Fact]
    public void IsMoveValid_ShouldReturnTrue_WhenFirstTurnCoversCenter()
    {
        var game = new Game();
        var player = new GuestPlayer("Olof");
        game.Players.Add(player);
        
        game.DrawTiles(player, 7);
        
        var placements = new List<TilePlacement>
        {
            new(7, 7, player.Rack[0]),
            new(7, 8, player.Rack[1]),
            new(7, 9, player.Rack[2])
        };

        var isValid = _gameLogic.ValidateMove(game, player, placements);

        Assert.True(isValid);
    }

    [Fact]
    public void IsMoveValid_ShouldReturnFalse_WhenFirstTurnDoesNotCoverCenter()
    {
        var game = new Game(); 
        var player = new GuestPlayer("Olof");
        var tile = game.TileBag[0];
        
        var placements = new List<TilePlacement>
        {
            new(0, 0, tile),
            new(0, 1, tile),
            new(0, 2, tile)
        };

        var isValid = _gameLogic.ValidateMove(game, player, placements);

        Assert.False(isValid); 
    }

    [Fact]
    public void IsMoveValid_ShouldReturnFalse_WhenTilesAreDiagonal()
    {
        var game = new Game();
        var player = new GuestPlayer("Olof");
        var tile = game.TileBag[0];
        
        var placements = new List<TilePlacement>
        {
            new(7, 7, tile),
            new(8, 8, tile),
            new(9, 9, tile)
        };

        var isValid = _gameLogic.ValidateMove(game, player, placements);

        Assert.False(isValid);
    }

    [Fact]
    public void IsMoveValid_ShouldReturnFalse_WhenTilesAreInLShape()
    {
        var game = new Game();
        var player = new GuestPlayer("Olof");
        var tile = game.TileBag[0];
        
        var placements = new List<TilePlacement>
        {
            new(7, 7, tile), 
            new(7, 8, tile), 
            new(8, 7, tile)  
        };

        var isValid = _gameLogic.ValidateMove(game, player, placements);

        Assert.False(isValid);
    }

    [Fact]
    public void IsMoveValid_ShouldReturnFalse_WhenWordHasGaps()
    {
        var game = new Game();
        var player = new GuestPlayer("Olof");
        var tile = game.TileBag[0];
        
        var placements = new List<TilePlacement>
        {
            new(7, 7, tile),
            new(7, 9, tile)
        };

        var isValid = _gameLogic.ValidateMove(game, player, placements);

        Assert.False(isValid);
    }

    [Fact]
    public void IsMoveValid_ShouldReturnTrue_WhenGapIsFilledByExistingBoardTile()
    {
        var game = new Game();
        var player = new GuestPlayer("Olof");
        game.Players.Add(player);
        game.DrawTiles(player, 7);
        
        game.Board.Squares[7, 8].Tile = game.TileBag[1];
        // Ensure center is technically covered so we pass the first turn rule
        game.Board.Squares[7, 7].Tile = game.TileBag[2]; 

        // Player places tiles on either side of the existing 7,8 tile
        var placements = new List<TilePlacement>
        {
            new(7, 6, player.Rack[0]),
            new(7, 9, player.Rack[1])
        };

        var isValid = _gameLogic.ValidateMove(game, player, placements);

        Assert.True(isValid);
    }

    [Fact]
    public void IsMoveValid_ShouldReturnFalse_WhenNotConnectedToExistingWords()
    {
        var game = new Game();
        var player = new GuestPlayer("Olof");
        var tile = game.TileBag[0];
        
        game.Board.Squares[7, 7].Tile = game.TileBag[1];

        var placements = new List<TilePlacement>
        {
            new(0, 0, tile),
            new(0, 1, tile)
        };

        var isValid = _gameLogic.ValidateMove(game, player, placements);

        Assert.False(isValid);
    }

    [Fact]
    public void IsMoveValid_ShouldReturnTrue_WhenConnectedToExistingWord()
    {
        var game = new Game();
        var player = new GuestPlayer("Olof");
        game.Players.Add(player);
        game.DrawTiles(player, 7);
        
        game.Board.Squares[7, 7].Tile = game.TileBag[1];

        var placements = new List<TilePlacement>
        {
            new(6, 7, player.Rack[0]),
            new(5, 7, player.Rack[1])
        };

        var isValid = _gameLogic.ValidateMove(game, player, placements);

        Assert.True(isValid);
    }
    
    [Fact]
    public void IsMoveValid_ShouldReturnFalse_WhenPlacementIsOutOfBounds()
    {
        var game = new Game();
        var player = new GuestPlayer("Olof");
        var tile = game.TileBag[0];
        
        var placements = new List<TilePlacement>
        {
            new(15, 7, tile) 
        };

        var isValid = _gameLogic.ValidateMove(game, player, placements);

        Assert.False(isValid);
    }

    [Fact]
    public void IsMoveValid_ShouldReturnFalse_WhenSquareIsAlreadyOccupied()
    {
        var game = new Game();
        var player = new GuestPlayer("Olof");
        var tile1 = game.TileBag[0];
        var tile2 = game.TileBag[1];

        game.Board.Squares[7, 7].Tile = tile1;

        var placements = new List<TilePlacement>
        {
            new(7, 7, tile2) 
        };

        var isValid = _gameLogic.ValidateMove(game, player, placements);

        Assert.False(isValid);
    }
    
    [Fact]
    public void IsMoveValid_ShouldExtractMainWord_AndCrossWords()
    {
        var spyDictionary = new SpyDictionaryService();
        var gameLogic = new GameLogicService(spyDictionary); 
    
        var game = new Game();
        var player = new GuestPlayer("Olof");
        game.Players.Add(player);

        game.Board.Squares[7, 7].Tile = new Tile { Letter = 'E', Value = 1 };
        game.Board.Squares[6, 6].Tile = new Tile { Letter = 'O', Value = 2 };

        var tileH = new Tile { Letter = 'H', Value = 4 };
        var tileJ = new Tile { Letter = 'J', Value = 7 };
    
        player.Rack.AddRange([tileH, tileJ]);
    
        var placements = new List<TilePlacement>
        {
            new(7, 6, tileH),
            new(7, 8, tileJ)
        };

        var isValid = gameLogic.ValidateMove(game, player, placements);

        Assert.True(isValid);
    
        Assert.Contains("HEJ", spyDictionary.CheckedWords);
        Assert.Contains("OH", spyDictionary.CheckedWords);
    
        Assert.Equal(2, spyDictionary.CheckedWords.Count); 
    }
    
    [Fact]
    public void CalculateScore_ShouldReturnBasicSum_WhenNoMultipliers()
    {
        var gameLogic = new GameLogicService(new SpyDictionaryService());
        var game = new Game();

        var placements = new List<TilePlacement>
        {
            new(0, 4, new Tile { Letter = 'H', Value = 3 }),
            new(0, 5, new Tile { Letter = 'E', Value = 1 }),
            new(0, 6, new Tile { Letter = 'J', Value = 7 })
        };

        var score = gameLogic.CalculateScore(game, placements);

        Assert.Equal(11, score);
    }

    [Fact]
    public void CalculateScore_ShouldApplyMultipliers_OnlyToNewTiles()
    {
        var gameLogic = new GameLogicService(new SpyDictionaryService());
        var game = new Game(); 

        game.Board.Squares[14, 0].Tile = new Tile { Letter = 'A', Value = 1 };

        var placements = new List<TilePlacement>
        {
            new(14, 1, new Tile { Letter = 'R', Value = 1 }),
            new(14, 2, new Tile { Letter = 'M', Value = 3 }),
            new(14, 3, new Tile { Letter = 'S', Value = 1 }) 
        };

        var score = gameLogic.CalculateScore(game, placements);

        Assert.Equal(7, score); 
    }

    [Fact]
    public void CalculateScore_ShouldAdd50PointBingoBonus_When7TilesPlaced()
    {
        var gameLogic = new GameLogicService(new SpyDictionaryService());
        var game = new Game();
    
        var placements = new List<TilePlacement>();
        for (var i = 0; i < 7; i++)
        {
            placements.Add(new TilePlacement(7, 7 + i, new Tile { Letter = 'A', Value = 1 }));
        }

        var score = gameLogic.CalculateScore(game, placements);

        Assert.Equal(66, score); // double word + 8 points for tiles (1 dl) + 50 point bingo bonus
    }
    
    [Fact]
    public void ExecuteMove_ShouldUpdateBoard_Score_Rack_AndAdvanceTurn()
    {
        var gameLogic = new GameLogicService(new SpyDictionaryService());
        var game = new Game();
    
        var player1 = new GuestPlayer("Olof");
        var player2 = new GuestPlayer("Gudrun");
        game.Players.Add(player1);
        game.Players.Add(player2);

        Assert.Equal(player1.Id, game.CurrentPlayerId);

        var initialBagCount = game.TileBag.Count;

        var tileA = new Tile { Letter = 'A', Value = 1 };
        var tileB = new Tile { Letter = 'B', Value = 3 };
        player1.Rack.AddRange([tileA, tileB, new Tile(), new Tile(), new Tile(), new Tile(), new Tile()]);

        var placements = new List<TilePlacement>
        {
            new(0, 4, tileA),
            new(0, 5, tileB)
        };

        gameLogic.ExecuteMove(game, player1, placements);

        Assert.Equal(4, player1.Score); 
        Assert.NotNull(game.Board.Squares[0, 4].Tile); 
        Assert.Equal('A', game.Board.Squares[0, 4].Tile!.Letter);

        Assert.Equal(7, player1.Rack.Count); 
        Assert.DoesNotContain(tileA, player1.Rack); 
        Assert.Equal(initialBagCount - 2, game.TileBag.Count); 

        Assert.Equal(player2.Id, game.CurrentPlayerId);
    }
}

public class FakeDictionaryService : IWordDictionaryService // A simple fake implementation that considers all words valid, since we're only testing move validation logic here
{
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public bool IsValidWord(string word, string language)
    {
        return true; 
    }
}

public class SpyDictionaryService : IWordDictionaryService // we use this spy to capture the words that GameLogicService checks for validity, so we can assert on them in our tests
{
    public List<string> CheckedWords { get; } = [];

    public Task InitializeAsync() => Task.CompletedTask;

    public bool IsValidWord(string word, string language)
    {
        CheckedWords.Add(word); 
        return true;            
    }
}