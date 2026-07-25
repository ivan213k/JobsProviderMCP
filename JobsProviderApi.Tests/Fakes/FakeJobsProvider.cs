using JobsProviderApi.Models;
using JobsProviderApi.Providers;

namespace JobsProviderApi.Tests.Fakes;

/// <summary>
/// Returns a fixed job list for any source, so a service under test can be driven without a real provider.
/// Implements every source marker, mirroring <c>MockJobsProvider</c>.
/// </summary>
internal sealed class FakeJobsProvider(IReadOnlyList<Job> jobs) : IIndeedJobsProvider, IStepstoneJobsProvider
{
    public Task<IReadOnlyList<Job>> GetJobsAsync(JobSearchQuery query, CancellationToken cancellationToken = default) =>
        Task.FromResult(jobs);
}
