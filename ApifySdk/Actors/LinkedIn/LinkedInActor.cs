using ApifySdk.Actors.LinkedIn.Models;

namespace ApifySdk.Actors.LinkedIn;

public class LinkedInActor : ILinkedInActor
{
    private const string ActorId = "cheap_scraper~linkedin-job-scraper";

    private IApifyApiClient _apifyApiClient;

    public LinkedInActor(IApifyApiClient apifyApiClient)
    {
        _apifyApiClient = apifyApiClient;
    }

    public async Task<IEnumerable<LinkedInJobResult>> SearchAsync(LinkedInSearchRequest searchRequest, CancellationToken cancellationToken)
    {
        return await _apifyApiClient.PostAsync<LinkedInJobResult, LinkedInSearchRequest>(ActorId, searchRequest, cancellationToken);
    }
}
