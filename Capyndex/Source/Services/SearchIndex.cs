using Capyndex.Models;
using System.Collections.Concurrent;

namespace Capyndex.Services;

public sealed class SearchIndex
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, int>> _invertedIndex = new();
    private readonly ConcurrentDictionary<Guid, string> _documents = new();

    public void IndexDocument(Document document)
    {
        _documents.TryAdd(document.Id, document.Content);
        var tokens = Tokenizer.Tokenize(document.Content);

        var tokenFrequencies = new Dictionary<string, int>();

        foreach (var token in tokens)
        {
            tokenFrequencies[token] = tokenFrequencies.GetValueOrDefault(token, 0) + 1;
        }

        foreach (var (token, frequency) in tokenFrequencies)
        {
            _invertedIndex.AddOrUpdate(
                token,
                _ => new([KeyValuePair.Create(document.Id, frequency)]),
                (_, existing) =>
                {
                    existing[document.Id] = frequency;

                    return existing;
                });
        }
    }

    // public List<SearchResult> Search(string query)
    // {
    //     var tokens = Tokenizer.Tokenize(query);
    //     var scoreMap = new Dictionary<Guid, int>();
    //
    //     foreach (var token in tokens)
    //     {
    //         if (!_invertedIndex.TryGetValue(token, out var docFrequencies))
    //         {
    //             continue;
    //         }
    //
    //         foreach (var (docId, frequency) in docFrequencies)
    //         {
    //             scoreMap[docId] = scoreMap.GetValueOrDefault(docId, 0) + frequency;
    //         }
    //     }
    //
    //     return scoreMap
    //            .Select(x => new SearchResult { Id = x.Key, Score = x.Value })
    //            .OrderByDescending(x => x.Score)
    //            .ToList();
    // }

    public Document? GetDocument(Guid id)
    {
        var exists = _documents.TryGetValue(id, out var content);

        if (!exists || content is null)
        {
            return null;
        }

        return new() { Id = id, Content = content };
    }
}