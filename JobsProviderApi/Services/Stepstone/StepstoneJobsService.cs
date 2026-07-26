using JobsProviderApi.Configuration;
using JobsProviderApi.Models;
using JobsProviderApi.Providers;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;

namespace JobsProviderApi.Services.Stepstone;

/// <summary>
/// Serves everything after the first 50 jobs from <see cref="IJobsProvider"/> as the Stepstone-sourced slice,
/// filtered by <see cref="JobSearchQuery"/>. Search responses are cached per query (see
/// <see cref="JobSearchQuery.ToCacheKey"/>) for <see cref="CachingOptions.SearchResultsDuration"/>; every job
/// returned from a search page is also cached individually (see <see cref="Job.ToCacheKey"/>) for
/// <see cref="CachingOptions.JobDuration"/>, which is what <see cref="GetByIdAsync"/> reads from.
/// </summary>
public class StepstoneJobsService(
    IIndeedJobsProvider jobsProvider,
    IJobSearchFilter jobSearchFilter,
    IFusionCache cache,
    IOptions<CachingOptions> cachingOptions) : IStepstoneJobsService
{
    private const int Skip = 50;
    private const string CacheKeySource = "stepstone";

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
        ListResponse<Job> result = jobSearchFilter.Apply(jobs.Skip(Skip), query);
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
