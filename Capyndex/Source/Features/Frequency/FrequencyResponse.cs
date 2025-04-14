using Capyndex.Models;

namespace Capyndex.Features.Frequency;

public sealed record FrequencyResponse(IReadOnlyList<SearchResult> Results);