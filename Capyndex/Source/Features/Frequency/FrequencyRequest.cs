using FluentValidation;

namespace Capyndex.Features.Frequency;

public sealed class FrequencyRequest
{
    public required string Query { get; init; }

    internal sealed class Validator : Validator<FrequencyRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Query).NotEmpty();
            RuleFor(x => x.Query).MaximumLength(100);
            RuleFor(x => x.Query)
                .Must(Tokenizer.IsSingleToken)
                .WithMessage("Query must contain exactly one search term without any special characters or spaces");
        }
    }
}