using JobsProviderApi.Mcp;
using JobsProviderApi.Models;
using JobsProviderApi.Services;
using JobsProviderApi.Services.Stepstone;
using JobsProviderApi.Tests.Fakes;
using ModelContextProtocol;

namespace JobsProviderApi.Tests.Mcp;

public class StepstoneJobSearchToolTests
{
    [Fact]
    public async Task SearchStepstoneJobsAsync_WithValidQuery_ReturnsFilteredJobs()
    {
        // StepstoneJobsService skips the first 50 jobs, so the fake provider needs at least that many filler jobs
        // ahead of the ones under test.
        List<Job> jobs = Enumerable.Range(1, 50).Select(id => TestJobs.Create(id)).ToList();
        jobs.Add(TestJobs.Create(51, title: "Senior Go Engineer"));
        jobs.Add(TestJobs.Create(52, title: "Java Engineer"));
        IStepstoneJobsService service = new StepstoneJobsService(new FakeJobsProvider(jobs), new JobSearchFilter());

        IReadOnlyList<Job> result = await StepstoneJobSearchTool.SearchStepstoneJobsAsync(
            service,
            search: "Go",
            mustHaveSkills: null,
            preferredSkills: null,
            locations: null,
            countryCode: "DE");

        Assert.Equal([51], result.Select(j => j.Id));
    }

    [Fact]
    public async Task SearchStepstoneJobsAsync_WithInvalidRegex_ThrowsMcpException()
    {
        IStepstoneJobsService service = new StepstoneJobsService(new FakeJobsProvider([]), new JobSearchFilter());

        await Assert.ThrowsAsync<McpException>(() => StepstoneJobSearchTool.SearchStepstoneJobsAsync(
            service,
            search: "[",
            mustHaveSkills: null,
            preferredSkills: null,
            locations: null,
            countryCode: "DE"));
    }
}
