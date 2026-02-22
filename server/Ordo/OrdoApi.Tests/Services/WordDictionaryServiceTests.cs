using Microsoft.Extensions.Logging.Abstractions;
using OrdoApi.Services;

namespace OrdoApi.Tests.Services;

// https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices

public class WordDictionaryServiceTests
{
    private readonly WordDictionaryService _service;

    // The constructor runs once before every test method
    public WordDictionaryServiceTests()
    {
        // NullLogger is a dummy logger that does nothing, useful for testing
        var logger = NullLogger<WordDictionaryService>.Instance;
        _service = new WordDictionaryService(logger);
    }

    [Fact]
    public async Task InitializeAsync_ShouldLoadDictionaries_WithoutThrowing()
    {
        // Act
        var exception = await Record.ExceptionAsync(() => _service.InitializeAsync());

        // Assert
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("katt", "swedish", true)]         
    [InlineData("KATT", "swedish", true)]        
    [InlineData("Katt", "swedish", true)]         
    [InlineData("notarealword", "swedish", false)]
    public async Task IsValidWord_ShouldValidateCorrectly(string word, string language, bool expected)
    {
        // Arrange
        await _service.InitializeAsync();

        // Act
        var result = _service.IsValidWord(word, language);

        // Assert
        Assert.Equal(expected, result);
    }
}