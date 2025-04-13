using Capyndex.Features.Upload;

namespace Capyndex.Models;

public sealed class Document
{
    public required Guid Id { get; init; }
    public required string Content { get; init; }
}

public static class DocumentExtensions
{
    public static Document NewFromRequest(UploadRequest request)
    {
        return new Document
        {
            Id = Guid.NewGuid(),
            Content = request.Content
        };
    }
}

