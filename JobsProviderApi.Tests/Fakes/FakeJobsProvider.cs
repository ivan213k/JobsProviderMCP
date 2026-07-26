using JobsProviderApi.Models;
using JobsProviderApi.Providers;

namespace JobsProviderApi.Tests.Fakes;

internal sealed class FakeJobsProvider(IReadOnlyList<Job> jobs) : IIndeedJobsProvider, IStepstoneJobsProvider
{
    public int CallCount { get; private set; }

    public Task<IReadOnlyList<Job>> GetJobsAsync(JobSearchQuery query, CancellationToken cancellationToken = default)
    {
        CallCount++;
        return Task.FromResult(jobs);
    }
}
