using JobsProviderApi.Models;

namespace JobsProviderApi.Services.Stepstone;

public interface IStepstoneJobsService
{
    Task<ListResponse<Job>> SearchAsync(JobSearchQuery query, CancellationToken cancellationToken = default);
}
