using JobsProviderApi.Configuration;
using JobsProviderApi.Models;
using JobsProviderApi.Providers;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;

namespace JobsProviderApi.Services.Indeed;

/// <summary>
/// Serves Indeed jobs from <see cref="IIndeedJobsProvider"/> (which fetches from the Apify actor and caches the
/// raw result), narrowed by <see cref="JobSearchQuery"/>'s skill/location filters — free-text search is applied
/// upstream by the actor, not here. Search responses are cached per query (see
/// <see cref="JobSearchQuery.ToCacheKey"/>) for <see cref="CachingOptions.SearchResultsDuration"/>; every job
/// returned from a search page is also cached individually (see <see cref="Job.ToCacheKey"/>) for
/// <see cref="CachingOptions.JobDuration"/>, which is what <see cref="GetByIdAsync"/> reads from.
/// </summary>
public class IndeedJobsService(
    IIndeedJobsProvider jobsProvider,
    IJobSearchFilter jobSearchFilter,
    IFusionCache cache,
    IOptions<CachingOptions> cachingOptions) : IIndeedJobsService
{
    private const string CacheKeySource = "indeed";

    public async Task<ListResponse<Job>> SearchAsync(JobSearchQuery query, CancellationToken cancellationToken = default) =>
        await cache.GetOrSetAsync(
            query.ToCacheKey(CacheKeySource),
            ct => SearchUncachedAsync(query, ct),
            token: cancellationToken);

    public async Task<Job?> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
        (await cache.TryGetAsync<Job>(Job.ToCacheKey(CacheKeySource, id), token: cancellationToken)).GetValueOrDefault();

    private async Task<ListResponse<Job>> SearchUncachedAsync(JobSearchQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<Job> jobs = await jobsProvider.GetJobsAsync(query, cancellationToken);
        ListResponse<Job> result = jobSearchFilter.Apply(jobs, query);
        await CacheJobsByIdAsync(result.Items, cancellationToken);
        return result;
    }

    private async Task CacheJobsByIdAsync(IReadOnlyList<Job> jobs, CancellationToken cancellationToken)
    {
        FusionCacheEntryOptions jobEntryOptions = cache.DefaultEntryOptions.Duplicate(cachingOptions.Value.JobDuration);
        foreach (Job job in jobs)
        {
            await cache.SetAsync(Job.ToCacheKey(CacheKeySource, job.Id), job, jobEntryOptions, token: cancellationToken);
        }
    }
}
