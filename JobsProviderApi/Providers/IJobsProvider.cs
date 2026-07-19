using JobsProviderApi.Models;

namespace JobsProviderApi.Providers;

public interface IJobsProvider
{
    Task<IReadOnlyList<Job>> GetJobsAsync(CancellationToken cancellationToken = default);
}
