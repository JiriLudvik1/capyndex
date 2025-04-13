namespace Capyndex.Services;

public static class Tokenizer
{
    public static IEnumerable<string> Tokenize(string text)
    {
        return text
            .ToLowerInvariant()
            .Split([' ', '.', ',', ';', ':', '-', '_', '/', '\\', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim());
    }
}