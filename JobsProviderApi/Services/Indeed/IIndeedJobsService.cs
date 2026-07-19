using JobsProviderApi.Models;

namespace JobsProviderApi.Services.Indeed;

public interface IIndeedJobsService
{
    Task<IReadOnlyList<Job>> SearchAsync(JobSearchQuery query, CancellationToken cancellationToken = default);
}
