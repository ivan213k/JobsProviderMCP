using JobsProviderApi.Models;
using JobsProviderApi.Providers;

namespace JobsProviderApi.Services.Stepstone;

/// <summary>
/// Serves everything after the first 50 jobs from the Stepstone <see cref="IJobsProvider{TSource}"/>, filtered
/// by <see cref="JobSearchQuery"/>.
/// </summary>
public class StepstoneJobsService(IStepstoneJobsProvider jobsProvider, IJobSearchFilter jobSearchFilter) : IStepstoneJobsService
{
    private const int Skip = 50;

    public async Task<ListResponse<Job>> SearchAsync(JobSearchQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Job> jobs = await jobsProvider.GetJobsAsync(query, cancellationToken);
        return jobSearchFilter.Apply(jobs.Skip(Skip), query);
    }
}
