namespace Capyndex.Models;

public sealed record SearchResult
{
    public required Guid Id { get; init; }
    public required int Score { get; init; }
}