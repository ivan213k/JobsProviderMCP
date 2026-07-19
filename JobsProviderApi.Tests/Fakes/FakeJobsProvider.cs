using JobsProviderApi.Models;
using JobsProviderApi.Providers;

namespace JobsProviderApi.Tests.Fakes;

internal sealed class FakeJobsProvider(IReadOnlyList<Job> jobs) : IJobsProvider
{
    public Task<IReadOnlyList<Job>> GetJobsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(jobs);
}
