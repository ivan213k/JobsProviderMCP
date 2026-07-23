using JobsProviderApi.Models;
using JobsProviderApi.Providers;

namespace JobsProviderApi.Services.Stepstone;

/// <summary>
/// Serves everything after the first 50 jobs from <see cref="IJobsProvider"/> as the Stepstone-sourced slice,
/// filtered by <see cref="JobSearchQuery"/>.
/// </summary>
public class StepstoneJobsService(IJobsProvider jobsProvider, IJobSearchFilter jobSearchFilter) : IStepstoneJobsService
{
    private const int Skip = 50;

    public async Task<ListResponse<Job>> SearchAsync(JobSearchQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Job> jobs = await jobsProvider.GetJobsAsync(cancellationToken);
        return jobSearchFilter.Apply(jobs.Skip(Skip), query);
    }
}
