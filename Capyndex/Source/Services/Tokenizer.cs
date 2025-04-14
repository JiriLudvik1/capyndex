public static class Tokenizer
{
    private static readonly char[] _splitChars = [' ', '.', ',', ';', ':', '-', '_', '/', '\\', '\n', '\r', '\t'];

    public static IEnumerable<string> Tokenize(string text)
    {
        return text
               .ToLowerInvariant()
               .Split(_splitChars, StringSplitOptions.RemoveEmptyEntries)
               .Select(x => x.Trim());
    }

    public static bool IsSingleToken(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        if (!text.Any(c => _splitChars.Contains(c)))
            return true;

        var lowered = text.ToLowerInvariant().Trim();
        var hasAnySplitChar = lowered.Any(c => Array.IndexOf(_splitChars, c) >= 0);

        if (!hasAnySplitChar)
            return true;

        var tokenCount = 0;

        foreach (var _ in text.ToLowerInvariant().Split(_splitChars, StringSplitOptions.RemoveEmptyEntries))
        {
            tokenCount++;

            if (tokenCount > 1)
                return false;
        }

        return tokenCount == 1;
    }
}