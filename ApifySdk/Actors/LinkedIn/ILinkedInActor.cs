using ApifySdk.Actors.LinkedIn.Models;

namespace ApifySdk.Actors.LinkedIn;

public interface ILinkedInActor
{
    public Task<IEnumerable<LinkedInJobResult>> SearchAsync(LinkedInSearchRequest searchRequest, CancellationToken cancellationToken);
}
