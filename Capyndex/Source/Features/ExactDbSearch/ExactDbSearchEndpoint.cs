using Capyndex.Database;
using Capyndex.Features.Search;
using Microsoft.EntityFrameworkCore;

namespace Capyndex.Features.ExactDbSearch;

public class ExactDbSearchEndpoint(AppDbContext context) : Endpoint<SearchRequest, SearchResponse>
{
    public override void Configure()
    {
        Post("/search-db");
        AllowAnonymous();
    }

    public override async Task HandleAsync(SearchRequest request, CancellationToken ct)
    {
        var result = await context.Documents.Where(x => x.Content.Contains(request.Query)).ToArrayAsync(ct);
        await SendAsync(new(result.Select(d => d.Id.ToString()).ToArray()), cancellation: ct);
    }
}