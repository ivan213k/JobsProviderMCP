using JobsProviderApi.Models;

namespace JobsProviderApi.Providers;

public interface IJobsProvider
{
    Task<IReadOnlyList<Job>> GetJobsAsync(JobSearchQuery query, CancellationToken cancellationToken = default);
}

public interface IJobsProvider<TSource> : IJobsProvider
    where TSource : IJobSource;
