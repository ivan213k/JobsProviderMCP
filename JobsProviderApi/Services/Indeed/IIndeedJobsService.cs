using JobsProviderApi.Models;

namespace JobsProviderApi.Services.Indeed;

public interface IIndeedJobsService
{
    Task<ListResponse<Job>> SearchAsync(JobSearchQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Looks up a single previously-searched job by id from cache only — no fallback fetch. Returns
    /// <see langword="null"/> if the job was never returned by a search, or its cache entry has expired.
    /// </summary>
    Task<Job?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
