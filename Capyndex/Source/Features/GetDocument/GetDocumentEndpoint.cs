using Capyndex.Models;
using Capyndex.Services;

namespace Capyndex.Features.GetDocument;

public sealed record GetDocumentRequest
{
    public Guid Id { get; init; }
}

public sealed record GetDocumentResponse(Document Document);

public class GetDocumentEndpoint(SearchIndex searchIndexService) : Endpoint<GetDocumentRequest, GetDocumentResponse>
{
    public override void Configure()
    {
        Get("/get-document/{Id}");
        AllowAnonymous();
    }
    
    public override async Task HandleAsync(GetDocumentRequest request, CancellationToken ct)
    {
        var document = searchIndexService.GetDocument(request.Id);

        if (document is null)
        {
            await SendNotFoundAsync();
            return;
        }
        await SendAsync(new(document));
    }
}