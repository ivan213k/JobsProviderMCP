using JobsProviderApi.Models;
using JobsProviderApi.Providers;
using ZiggyCreatures.Caching.Fusion;

namespace JobsProviderApi.Services.Stepstone;

/// <summary>
/// Serves everything after the first 50 jobs from <see cref="IJobsProvider"/> as the Stepstone-sourced slice,
/// filtered by <see cref="JobSearchQuery"/>. Responses are cached per query (see
/// <see cref="JobSearchQuery.ToCacheKey"/>) for the cache's default duration.
/// </summary>
public class StepstoneJobsService(IJobsProvider jobsProvider, IJobSearchFilter jobSearchFilter, IFusionCache cache) : IStepstoneJobsService
{
    private const int Skip = 50;
    private const string CacheKeySource = "stepstone";

    public async Task<ListResponse<Job>> SearchAsync(JobSearchQuery query, CancellationToken cancellationToken = default) =>
        await cache.GetOrSetAsync(
            query.ToCacheKey(CacheKeySource),
            ct => SearchUncachedAsync(query, ct),
            token: cancellationToken);

    private async Task<ListResponse<Job>> SearchUncachedAsync(JobSearchQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<Job> jobs = await jobsProvider.GetJobsAsync(cancellationToken);
        return jobSearchFilter.Apply(jobs.Skip(Skip), query);
    }
}
