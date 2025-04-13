using Capyndex.Database;
using Capyndex.Models;
using Capyndex.Services;
using Capyndex.Shared.Guards;
using Microsoft.EntityFrameworkCore;

namespace Capyndex.Features.GetDocument;

public sealed record GetDocumentRequest
{
    public Guid Id { get; init; }
}

public sealed record GetDocumentResponse(Document Document);

public class GetDocumentEndpoint(AppDbContext dbContext) : Endpoint<GetDocumentRequest, GetDocumentResponse>
{
    public override void Configure()
    {
        Get("/get-document/{Id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetDocumentRequest request, CancellationToken ct)
    {
        var document = await dbContext.Documents.FirstOrDefaultAsync(r => r.Id == request.Id, ct);
        Guard.Against.NotFound(request.Id, document);
        await SendAsync(new(document), cancellation: ct);
    }
}