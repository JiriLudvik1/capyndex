using Capyndex.Services;

namespace Capyndex.Features.Search;

public class SearchEndpoint(SearchIndex searchIndexService) : Endpoint<SearchRequest, SearchResponse>
{
    public override void Configure()
    {
        Post("/search");
        AllowAnonymous();
    }

    public override async Task HandleAsync(SearchRequest request, CancellationToken ct)
    {
        var searchResults = searchIndexService.Search(request.Query);
        await SendAsync(new(searchResults), cancellation: ct);
    }
}