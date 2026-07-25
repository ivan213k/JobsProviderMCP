using JobsProviderApi.Models;
using JobsProviderApi.Providers;
using ZiggyCreatures.Caching.Fusion;

namespace JobsProviderApi.Services.Indeed;

/// <summary>
/// Serves the first 50 jobs from <see cref="IJobsProvider"/> as the Indeed-sourced slice, filtered by
/// <see cref="JobSearchQuery"/>. Responses are cached per query (see <see cref="JobSearchQuery.ToCacheKey"/>)
/// for the cache's default duration.
/// </summary>
public class IndeedJobsService(IJobsProvider jobsProvider, IJobSearchFilter jobSearchFilter, IFusionCache cache) : IIndeedJobsService
{
    private const int SliceSize = 50;
    private const string CacheKeySource = "indeed";

    public async Task<ListResponse<Job>> SearchAsync(JobSearchQuery query, CancellationToken cancellationToken = default) =>
        await cache.GetOrSetAsync(
            query.ToCacheKey(CacheKeySource),
            ct => SearchUncachedAsync(query, ct),
            token: cancellationToken);

    private async Task<ListResponse<Job>> SearchUncachedAsync(JobSearchQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<Job> jobs = await jobsProvider.GetJobsAsync(cancellationToken);
        return jobSearchFilter.Apply(jobs.Take(SliceSize), query);
    }
}
