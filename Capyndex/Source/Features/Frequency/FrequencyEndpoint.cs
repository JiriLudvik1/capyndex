using Capyndex.Services;

namespace Capyndex.Features.Frequency;

public class FrequencyEndpoint(RedisIndexService redisIndexService)
    : Endpoint<FrequencyRequest, FrequencyResponse>
{
    public override void Configure()
    {
        Post("/frequency");
        AllowAnonymous();
    }

    public override async Task HandleAsync(FrequencyRequest request, CancellationToken ct)
    {
        var searchResults = await redisIndexService.GetDocumentTermFrequenciesAsync(request.Query);
        await SendAsync(new(searchResults), cancellation: ct);
    }
}