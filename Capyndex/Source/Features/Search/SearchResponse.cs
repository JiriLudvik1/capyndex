using Capyndex.Models;

namespace Capyndex.Features.Search;

public sealed record SearchResponse(IReadOnlyList<SearchResult> Results);
