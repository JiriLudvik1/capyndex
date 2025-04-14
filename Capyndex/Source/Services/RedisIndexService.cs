using Capyndex.Models;
using StackExchange.Redis;

namespace Capyndex.Services;

public class RedisIndexService(IConnectionMultiplexer redis)
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task AddToIndexAsync(Document document)
    {
        var terms = Tokenizer.Tokenize(document.Content);
        List<Task> tasks = [];

        foreach (var group in terms.GroupBy(t => t))
        {
            var term = group.Key;
            var count = group.Count();

            var key = $"index:{term}";
            tasks.Add(_db.HashSetAsync(key, document.Id.ToString(), count));
        }
        await Task.WhenAll(tasks);
    }

    public async Task<List<SearchResult>> GetDocumentTermFrequenciesAsync(string term)
    {
        var key = $"index:{term}";
        var entries = await _db.HashGetAllAsync(key);

        return entries
               .Select(e => new SearchResult { Id = Guid.Parse(e.Name!), Score = (int)e.Value! })
               .OrderByDescending(x => x.Score)
               .ToList();
    }

    public async Task<Guid[]> GetFullTextSearchAsync(string query)
    {
        var terms = Tokenizer.Tokenize(query).ToList();

        if (!terms.Any())
        {
            return [];
        }

        Dictionary<Guid, int> documentTermCounts = new();

        foreach (var term in terms)
        {
            var key = $"index:{term}";
            var entries = await _db.HashGetAllAsync(key);

            if (term == terms.First())
            {
                foreach (var entry in entries)
                {
                    var docId = Guid.Parse(entry.Name!);
                    documentTermCounts[docId] = 1;
                }

                continue;
            }

            foreach (var entry in entries)
            {
                var docId = Guid.Parse(entry.Name!);

                if (documentTermCounts.TryGetValue(docId, out var value))
                {
                    documentTermCounts[docId] = ++value;
                }
            }
        }

        // Return only document IDs that contain all terms (count equals the number of terms)
        return documentTermCounts
               .Where(kvp => kvp.Value == terms.Count)
               .Select(kvp => kvp.Key)
               .ToArray();
    }

    public async Task<bool> IsEmptyAsync()
    {
        var server = _db.Multiplexer.GetServer(_db.Multiplexer.GetEndPoints().First());

        return !server.Keys(pattern: "index:*").Any();
    }

    public async Task ClearIndexAsync()
    {
        var server = _db.Multiplexer.GetServer(_db.Multiplexer.GetEndPoints().First());
        var keys = server.Keys(pattern: "index:*");

        foreach (var key in keys)
        {
            await _db.KeyDeleteAsync(key);
        }
    }
}