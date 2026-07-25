using ZiggyCreatures.Caching.Fusion;

namespace JobsProviderApi.Endpoints.Cache;

internal static class ClearCacheHandler
{
    public static async Task<IResult> HandleAsync(IFusionCache cache, CancellationToken cancellationToken)
    {
        await cache.ClearAsync(allowFailSafe: false, token: cancellationToken);
        return Results.NoContent();
    }
}
