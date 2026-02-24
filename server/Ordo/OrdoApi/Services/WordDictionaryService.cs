namespace OrdoApi.Services;

public class WordDictionaryService(ILogger<WordDictionaryService> logger) : IWordDictionaryService
{
    private readonly Dictionary<string, HashSet<string>> _dictionaries = new();

    public async Task InitializeAsync()
    {
        var supportedLanguages = new[] { "swedish" }; // TODO: add english in the future
        foreach (var language in supportedLanguages)
        {
            await LoadDictionary(language);
        }
    }

    private async Task LoadDictionary(string language)
    {
        var basePath = AppContext.BaseDirectory;
        var path = Path.Combine(basePath, "Words", $"dictionary-{language}.txt");
        if (!File.Exists(path))
        {
            logger.LogWarning("Dictionary file for language {Language} not found at path: {Path}", language, path);
            return;
        }
        
        var words = await File.ReadAllLinesAsync(path);
        var wordSet = new HashSet<string>(words, StringComparer.OrdinalIgnoreCase);
        _dictionaries.Add(language, wordSet);
        logger.LogInformation("Loaded {WordCount} words for language {Language}.", wordSet.Count, language);
    }

    public bool IsValidWord(string word, string language)
    {
        return _dictionaries.ContainsKey(language)
               && _dictionaries[language].Contains(word);
    }
}