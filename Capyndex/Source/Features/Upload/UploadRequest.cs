using FluentValidation;

namespace Capyndex.Features.Upload;

public sealed class UploadRequest
{
    public string Content { get; init; }

    internal sealed class Validator : Validator<UploadRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Content).NotNull();
            RuleFor(x => x.Content).MinimumLength(1);
        }
    }
}