using JobsProviderApi.Models;
using JobsProviderApi.Providers;

namespace JobsProviderApi.Tests.Fakes;

/// <summary>
/// Returns a fixed job list for any source, so a service under test can be driven without a real provider.
/// </summary>
internal sealed class FakeJobsProvider<TSource>(IReadOnlyList<Job> jobs) : IJobsProvider<TSource>
    where TSource : IJobSource
{
    public Task<IReadOnlyList<Job>> GetJobsAsync(JobSearchQuery query, CancellationToken cancellationToken = default) =>
        Task.FromResult(jobs);
}
