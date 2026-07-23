namespace ApifySdk;

public interface IApifyApiClient
{
    Task<IEnumerable<T>> RunActorAsync<T>(string actorId, object input, CancellationToken cancellationToken = default);
}
