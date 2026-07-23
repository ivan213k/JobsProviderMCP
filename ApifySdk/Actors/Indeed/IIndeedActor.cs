using ApifySdk.Actors.Indeed.Models;

namespace ApifySdk.Actors.Indeed;

public interface IIndeedActor
{
    public Task<IEnumerable<IndeedJobResult>> SearchAsync(IndeedSearchRequest searchRequest, CancellationToken cancellationToken);
}
