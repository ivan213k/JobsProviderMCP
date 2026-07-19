using JobsProviderApi.Models;

namespace JobsProviderApi.Services.Stepstone;

public interface IStepstoneJobsService
{
    Task<IReadOnlyList<Job>> SearchAsync(JobSearchQuery query, CancellationToken cancellationToken = default);
}
