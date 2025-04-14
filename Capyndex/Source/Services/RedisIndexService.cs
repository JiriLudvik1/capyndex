using Capyndex.Database;
using Capyndex.Models;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Capyndex.Services;

public class RedisIndexService(IConnectionMultiplexer redis, IServiceProvider serviceProvider)
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task AddToIndexAsync(Document document)
    {
        // Use StringPool to reuse common string instances
        ReadOnlySpan<char> prefix = "index:";

        // Tokenize once and use memory-efficient data structures
        var terms = Tokenizer.Tokenize(document.Content).ToArray();

        // Convert document.Id to string once outside the loop
        var docIdString = document.Id.ToString();

        // Pre-allocate the list with expected capacity to avoid resizing
        int estimatedTermCount = Math.Min(terms.Length, 500); // Reasonable upper limit
        var tasks = new List<Task>(estimatedTermCount);

        // Use value type for string key to avoid allocations in dictionary lookups
        var termCountMap = new Dictionary<string, int>(StringComparer.Ordinal);

        // Count term frequencies without using GroupBy (more efficient)
        foreach (var term in terms)
        {
            if (termCountMap.TryGetValue(term, out int currentCount))
            {
                termCountMap[term] = currentCount + 1;
            }
            else
            {
                termCountMap[term] = 1;
            }
        }

        // Use ValueStringBuilder to construct keys without extra allocations
        Span<char> keyBuffer = stackalloc char[256]; // Adjust based on expected key size

        foreach (var pair in termCountMap)
        {
            // Reuse the buffer for each key construction
            prefix.CopyTo(keyBuffer);
            var term = pair.Key.AsSpan();
            term.CopyTo(keyBuffer[prefix.Length..]);

            // Create the Redis key from the buffer (only one allocation here)
            var key = new string(keyBuffer[..(prefix.Length + term.Length)]);

            tasks.Add(_db.HashSetAsync(key, docIdString, pair.Value));
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
        // Early return for empty tokens
        var terms = Tokenizer.Tokenize(query).ToList();

        if (terms.Count == 0)
        {
            return [];
        }

        // For extremely large queries, limit terms to the most significant ones
        if (terms.Count > 20) // You can adjust this threshold based on your needs
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var documents =
                await dbContext.Documents
                               .Where(d => d.Content.Contains(query))
                               .AsNoTracking()
                               .ToListAsync();

            if (documents.Count == 1)
            {
                return documents.Select(d => d.Id).ToArray();
            }

            // Keep only the first 20 terms or filter to keep longer, more significant terms
            terms = terms.Where(t => t.Length > 3)
                         .Take(20)
                         .ToList();

            if (terms.Count == 0) // If no significant terms remain
            {
                return [];
            }
        }

        // Process in batches to reduce Redis round trips
        const int batchSize = 5;
        var batchTasks = new List<Task<HashEntry[]>>();
        var keyToTermMap = new Dictionary<string, string>();

        // Prepare all Redis keys and batch the requests
        foreach (var term in terms)
        {
            var key = $"index:{term}";
            keyToTermMap[key] = term;
            batchTasks.Add(_db.HashGetAllAsync(key));

            // Process in batches to avoid overwhelming Redis
            if (batchTasks.Count >= batchSize)
            {
                await Task.WhenAll(batchTasks);
                batchTasks.Clear();
            }
        }

        // Process any remaining tasks
        if (batchTasks.Count > 0)
        {
            await Task.WhenAll(batchTasks);
        }

        // Get results from first term to initialize our set
        var firstKey = $"index:{terms[0]}";
        var firstEntries = await _db.HashGetAllAsync(firstKey);

        if (firstEntries.Length == 0 || terms.Count == 1)
        {
            return firstEntries.Select(entry => Guid.Parse(entry.Name!)).ToArray();
        }

        // Create a hashset with initial capacity to avoid resizing
        var matchingDocIds = new HashSet<Guid>(firstEntries.Length);

        foreach (var entry in firstEntries)
        {
            matchingDocIds.Add(Guid.Parse(entry.Name!));
        }

        foreach (var term in terms.Skip(1)) // Skip first term as we already processed it
        {
            var key = $"index:{term}";
            var entries = await _db.HashGetAllAsync(key);

            // Skip rare terms that appear in very few documents (optional)
            if (entries.Length < 3) // Adjust threshold as needed
            {
                continue;
            }

            // Convert entries to HashSet for efficient intersection
            var termDocIds = new HashSet<Guid>(entries.Length);

            foreach (var entry in entries)
            {
                termDocIds.Add(Guid.Parse(entry.Name!));
            }

            // Keep only documents that exist in both sets
            matchingDocIds.IntersectWith(termDocIds);

            // Early exit if no matches remain
            if (matchingDocIds.Count == 0)
            {
                break;
            }
        }

        return matchingDocIds.ToArray();
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