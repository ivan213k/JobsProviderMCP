using JobsProviderApi.Models;

namespace JobsProviderApi.Services.Indeed;

public interface IIndeedJobsService
{
    Task<ListResponse<Job>> SearchAsync(JobSearchQuery query, CancellationToken cancellationToken = default);
}
