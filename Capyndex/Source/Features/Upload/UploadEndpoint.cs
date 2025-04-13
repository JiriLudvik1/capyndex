using Capyndex.Models;
using Capyndex.Services;

namespace Capyndex.Features.Upload;

public class UploadEndpoint(SearchIndex searchIndexService, RedisIndexService redisIndexService)
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

        // searchIndexService.IndexDocument(document);
        redisIndexService.AddToIndexAsync(document);
        await SendAsync(new(document.Id), cancellation: ct);
    }
}