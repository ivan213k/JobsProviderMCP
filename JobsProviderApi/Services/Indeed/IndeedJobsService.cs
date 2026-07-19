using JobsProviderApi.Models;
using JobsProviderApi.Providers;

namespace JobsProviderApi.Services.Indeed;

/// <summary>
/// Serves the first 50 jobs from <see cref="IJobsProvider"/> as the Indeed-sourced slice, filtered by
/// <see cref="JobSearchQuery"/>.
/// </summary>
public class IndeedJobsService(IJobsProvider jobsProvider, IJobSearchFilter jobSearchFilter) : IIndeedJobsService
{
    private const int SliceSize = 50;

    public async Task<IReadOnlyList<Job>> SearchAsync(JobSearchQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Job> jobs = await jobsProvider.GetJobsAsync(cancellationToken);
        return jobSearchFilter.Apply(jobs.Take(SliceSize), query);
    }
}
