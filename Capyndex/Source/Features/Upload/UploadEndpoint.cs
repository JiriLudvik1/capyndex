using Capyndex.Database;
using Capyndex.Models;
using Capyndex.Services;

namespace Capyndex.Features.Upload;

public class UploadEndpoint(RedisIndexService redisIndexService, AppDbContext dbContext)
    : Endpoint<UploadRequest, UploadResponse>
{
    public override void Configure()
    {
        Post("/upload");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UploadRequest request, CancellationToken ct)
    {
        var document = DocumentExtensions.NewFromRequest(request);

        await dbContext.Documents.AddAsync(document, ct);
        await redisIndexService.AddToIndexAsync(document);
        await dbContext.SaveChangesAsync(ct);

        await SendAsync(new(document.Id), cancellation: ct);
    }
}