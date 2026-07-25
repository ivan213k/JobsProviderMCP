using JobsProviderApi.Mcp;
using JobsProviderApi.Models;
using JobsProviderApi.Services;
using JobsProviderApi.Services.Stepstone;
using JobsProviderApi.Tests.Fakes;

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
        IStepstoneJobsService service = new StepstoneJobsService(new FakeJobsProvider(jobs), new JobSearchFilter(), TestFusionCache.Create());

        ListResponse<Job> result = await StepstoneJobSearchTool.SearchStepstoneJobsAsync(
            service,
            search: "Go",
            countryCode: "DE");

        Assert.Equal(1, result.TotalCount);
        Assert.Equal([51], result.Items.Select(j => j.Id));
    }

    [Fact]
    public async Task SearchStepstoneJobsAsync_WithSpecialCharacters_MatchesAsPlainText()
    {
        List<Job> jobs = Enumerable.Range(1, 50).Select(id => TestJobs.Create(id)).ToList();
        jobs.Add(TestJobs.Create(51, title: "Backend Engineer (C++/Go)"));
        IStepstoneJobsService service = new StepstoneJobsService(new FakeJobsProvider(jobs), new JobSearchFilter(), TestFusionCache.Create());

        ListResponse<Job> result = await StepstoneJobSearchTool.SearchStepstoneJobsAsync(
            service,
            search: "(C++/Go)",
            countryCode: "DE");

        Assert.Equal([51], result.Items.Select(j => j.Id));
    }

    [Fact]
    public async Task SearchStepstoneJobsAsync_WithSkip_SkipsResults()
    {
        List<Job> jobs = Enumerable.Range(1, 55).Select(id => TestJobs.Create(id)).ToList();
        IStepstoneJobsService service = new StepstoneJobsService(new FakeJobsProvider(jobs), new JobSearchFilter(), TestFusionCache.Create());

        ListResponse<Job> result = await StepstoneJobSearchTool.SearchStepstoneJobsAsync(
            service,
            search: "",
            countryCode: "DE",
            skip: 3);

        Assert.Equal(5, result.TotalCount);
        Assert.Equal([54, 55], result.Items.Select(j => j.Id));
    }
}
