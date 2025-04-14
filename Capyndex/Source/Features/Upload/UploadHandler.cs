using Capyndex.Database;
using Capyndex.Models;
using Capyndex.Services;

namespace Capyndex.Features.Upload;

public class UploadDocument : ICommand<UploadResponse>
{
    public required UploadRequest Request { get; init; }
}

public class UploadHandler(RedisIndexService redisIndexService, AppDbContext dbContext)
    : ICommandHandler<UploadDocument, UploadResponse>
{
    public async Task<UploadResponse> ExecuteAsync(UploadDocument command, CancellationToken ct)
    {
        var document = DocumentExtensions.NewFromRequest(command.Request);

        await dbContext.Documents.AddAsync(document, ct);
        await redisIndexService.AddToIndexAsync(document);
        await dbContext.SaveChangesAsync(ct);

        return new(document.Id);
    }
}