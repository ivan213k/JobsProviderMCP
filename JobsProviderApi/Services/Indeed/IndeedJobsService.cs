using JobsProviderApi.Models;
using JobsProviderApi.Providers;

namespace JobsProviderApi.Services.Indeed;

/// <summary>
/// Serves the first 50 jobs from the Indeed <see cref="IJobsProvider{TSource}"/>, filtered by
/// <see cref="JobSearchQuery"/>.
/// </summary>
public class IndeedJobsService(IJobsProvider<IndeedSource> jobsProvider, IJobSearchFilter jobSearchFilter) : IIndeedJobsService
{
    private const int SliceSize = 50;

    public async Task<ListResponse<Job>> SearchAsync(JobSearchQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Job> jobs = await jobsProvider.GetJobsAsync(query, cancellationToken);
        return jobSearchFilter.Apply(jobs.Take(SliceSize), query);
    }
}
