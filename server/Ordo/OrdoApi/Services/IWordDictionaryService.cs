namespace OrdoApi.Services;

public interface IWordDictionaryService
{
    Task InitializeAsync();
    bool IsValidWord(string word, string language);
}