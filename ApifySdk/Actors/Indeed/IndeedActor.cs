using ApifySdk.Actors.Indeed.Models;

namespace ApifySdk.Actors.Indeed;

public class IndeedActor : IIndeedActor
{
    private const string ActorId = "valig~indeed-jobs-scraper";

    private IApifyApiClient _apifyApiClient;

    public IndeedActor(IApifyApiClient apifyApiClient)
    {
        _apifyApiClient = apifyApiClient;
    }

    public async Task<IEnumerable<IndeedJobResult>> SearchAsync(IndeedSearchRequest searchRequest, CancellationToken cancellationToken)
    {
        return await _apifyApiClient.PostAsync<IndeedJobResult>(ActorId, searchRequest, cancellationToken);
    }
}
