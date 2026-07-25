using JobsProviderApi.Configuration;
using JobsProviderApi.Models;
using JobsProviderApi.Providers;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;

namespace JobsProviderApi.Services.Indeed;

/// <summary>
/// Serves the first 50 jobs from <see cref="IJobsProvider"/> as the Indeed-sourced slice, filtered by
/// <see cref="JobSearchQuery"/>. Search responses are cached per query (see
/// <see cref="JobSearchQuery.ToCacheKey"/>) for <see cref="CachingOptions.SearchResultsDuration"/>; every job
/// returned from a search page is also cached individually (see <see cref="Job.ToCacheKey"/>) for
/// <see cref="CachingOptions.JobDuration"/>, which is what <see cref="GetByIdAsync"/> reads from.
/// </summary>
public class IndeedJobsService(
    IJobsProvider jobsProvider,
    IJobSearchFilter jobSearchFilter,
    IFusionCache cache,
    IOptions<CachingOptions> cachingOptions) : IIndeedJobsService
{
    private const int SliceSize = 50;
    private const string CacheKeySource = "indeed";

    public async Task<ListResponse<Job>> SearchAsync(JobSearchQuery query, CancellationToken cancellationToken = default) =>
        await cache.GetOrSetAsync(
            query.ToCacheKey(CacheKeySource),
            ct => SearchUncachedAsync(query, ct),
            token: cancellationToken);

    public async Task<Job?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        (await cache.TryGetAsync<Job>(Job.ToCacheKey(CacheKeySource, id), token: cancellationToken)).GetValueOrDefault();

    private async Task<ListResponse<Job>> SearchUncachedAsync(JobSearchQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<Job> jobs = await jobsProvider.GetJobsAsync(cancellationToken);
        ListResponse<Job> result = jobSearchFilter.Apply(jobs.Take(SliceSize), query);
        await CacheJobsByIdAsync(result.Items, cancellationToken);
        return result;
    }

    private async Task CacheJobsByIdAsync(IReadOnlyList<Job> jobs, CancellationToken cancellationToken)
    {
        var jobEntryOptions = new FusionCacheEntryOptions { Duration = cachingOptions.Value.JobDuration };
        foreach (Job job in jobs)
        {
            await cache.SetAsync(Job.ToCacheKey(CacheKeySource, job.Id), job, jobEntryOptions, token: cancellationToken);
        }
    }
}
