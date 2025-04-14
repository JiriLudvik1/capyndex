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
               .Select(e => new SearchResult { Id = e.Name!, Score = (int)e.Value! })
               .OrderByDescending(x => x.Score)
               .ToList();
    }

    public async Task<string[]> GetFullTextSearchAsync(string query)
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
            var documentIds =
                await dbContext.Documents
                               .Where(d => d.Content.Contains(query))
                               .AsNoTracking()
                               .Select(d => d.Id)
                               .ToListAsync();

            if (documentIds.Count == 1)
            {
                return documentIds.Select(x => x.ToString()).ToArray();
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

        // Process in smaller chunks to avoid timeouts
        const int pipelineChunkSize = 5;
        var allTermResults = new Dictionary<string, HashEntry[]>(terms.Count);
        var chunkTasks = new Dictionary<string, Task<HashEntry[]>>();

        for (int i = 0; i < terms.Count; i += pipelineChunkSize)
        {
            if (chunkTasks.Count > 0)
            {
                chunkTasks.Clear();
            }

            // Take a chunk of terms
            var chunkTerms = terms.Skip(i).Take(pipelineChunkSize).ToList();

            // Create a batch for this chunk
            var batch = _db.CreateBatch();

            // Add commands to the batch
            foreach (var term in chunkTerms)
            {
                var key = $"index:{term}";
                chunkTasks[term] = batch.HashGetAllAsync(key);
            }

            // Execute this batch
            batch.Execute();

            // Wait for all results in this chunk
            await Task.WhenAll(chunkTasks.Values);

            // Store the results
            foreach (var kvp in chunkTasks)
            {
                allTermResults[kvp.Key] = await kvp.Value;
            }
        }

        // Process results
        var firstTerm = terms[0];
        var firstEntries = allTermResults[firstTerm];

        if (firstEntries.Length == 0)
        {
            return []; // No matches for the first term
        }

        if (terms.Count == 1)
        {
            // Only perform Guid parsing once at the end for the single term scenario
            return firstEntries.Select(entry => entry.Name.ToString()).ToArray();
        }

        // Work with string keys instead of Guids for the intersection operations
        var matchingDocIdStrings = new HashSet<string>(
            firstEntries.Select(entry => entry.Name!.ToString()),
            StringComparer.Ordinal); // Using StringComparer.Ordinal for performance

        // Reusable hashset for intersection operations
        HashSet<string>? termDocIdStrings = null;

        foreach (var term in terms.Skip(1)) // Skip first term as we already processed it
        {
            var entries = allTermResults[term];

            // Skip rare terms that appear in very few documents (optional)
            if (entries.Length < 3) // Adjust threshold as needed
            {
                continue;
            }

            // Extract all names from entries in one go
            var entryNames = new string[entries.Length];

            for (int i = 0; i < entries.Length; i++)
            {
                entryNames[i] = entries[i].Name!;
            }

            // Reuse or create the hashset
            if (termDocIdStrings == null)
            {
                // Initialize with all entry names at once
                termDocIdStrings = new(entryNames, StringComparer.Ordinal);
            }
            else
            {
                termDocIdStrings.Clear(); // Clear for reuse

                // Add all entry names at once using UnionWith
                termDocIdStrings.UnionWith(entryNames);
            }

            // Keep only documents that exist in both sets
            matchingDocIdStrings.IntersectWith(termDocIdStrings);

            // Early exit if no matches remain
            if (matchingDocIdStrings.Count == 0)
            {
                return [];
            }
        }

        // Only convert to Guid array at the very end
        return matchingDocIdStrings.ToArray();
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