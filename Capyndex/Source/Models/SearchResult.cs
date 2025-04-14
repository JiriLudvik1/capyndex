namespace Capyndex.Models;

public sealed record SearchResult
{
    public required string Id { get; init; }
    public required int Score { get; init; }
}