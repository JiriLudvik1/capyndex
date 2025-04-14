using Capyndex.Services;

namespace Capyndex.Features.Search;

public sealed record SearchRequest(string Query);

public sealed record SearchResponse(string[] Results);

public class SearchEndpoint(RedisIndexService redisIndexService) : Endpoint<SearchRequest, SearchResponse>
{
    public override void Configure()
    {
        Post("/search");
        AllowAnonymous();
    }

    public override async Task HandleAsync(SearchRequest request, CancellationToken ct)
    {
        var result = await redisIndexService.GetFullTextSearchAsync(query: request.Query);
        await SendAsync(new(result), cancellation: ct);
    }
}