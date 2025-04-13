using Capyndex.Services;

namespace Capyndex.Features.Search;

public class SearchEndpoint(SearchIndex searchIndexService, RedisIndexService redisIndexService)
    : Endpoint<SearchRequest, SearchResponse>
{
    public override void Configure()
    {
        Post("/search");
        AllowAnonymous();
    }

    public override async Task HandleAsync(SearchRequest request, CancellationToken ct)
    {
        var searchResults = await redisIndexService.GetDocumentsForTermAsync(request.Query);
        await SendAsync(new(searchResults), cancellation: ct);
    }
}