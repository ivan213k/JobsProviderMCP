using System.Text.Json;
using JobsProviderApi.Models;

namespace JobsProviderApi.Providers;

public class MockJobsProvider<TSource>(IWebHostEnvironment environment) : IJobsProvider<TSource>
    where TSource : IJobSource
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<Job>> GetJobsAsync(JobSearchQuery query, CancellationToken cancellationToken = default)
    {
        string dataFilePath = Path.Combine(environment.ContentRootPath, "Data", "mock-jobs.json");
        await using FileStream stream = File.OpenRead(dataFilePath);
        MockJobsFile? file = await JsonSerializer.DeserializeAsync<MockJobsFile>(stream, JsonOptions, cancellationToken);
        return file?.Jobs ?? [];
    }

    private sealed record MockJobsFile(List<Job> Jobs);
}
