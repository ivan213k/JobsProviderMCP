namespace ApifySdk;

public interface IApifyApiClient
{
    Task<IEnumerable<T>> PostAsync<T>(string actorId, object input, CancellationToken cancellationToken = default);
}
