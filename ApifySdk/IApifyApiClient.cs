namespace ApifySdk;

public interface IApifyApiClient
{
    Task<IEnumerable<TResponse>> PostAsync<TResponse, TSearchQuery>(string actorId, TSearchQuery query, CancellationToken cancellationToken = default);
}
