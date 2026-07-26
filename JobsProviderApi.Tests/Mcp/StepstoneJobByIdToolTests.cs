using JobsProviderApi.Mcp;
using JobsProviderApi.Models;
using JobsProviderApi.Services;
using JobsProviderApi.Services.Stepstone;
using JobsProviderApi.Tests.Fakes;

namespace JobsProviderApi.Tests.Mcp;

public class StepstoneJobByIdToolTests
{
    [Fact]
    public async Task GetStepstoneJobAsync_AfterSearchReturnedTheJob_ReturnsIt()
    {
        // StepstoneJobsService skips the first 50 jobs, so the fake provider needs at least that many filler jobs
        // ahead of the one under test.
        List<Job> jobs = Enumerable.Range(1, 50).Select(id => TestJobs.Create(id)).ToList();
        jobs.Add(TestJobs.Create(51, title: "Senior Go Engineer"));
        IStepstoneJobsService service = new StepstoneJobsService(
            new FakeJobsProvider(jobs), new JobSearchFilter(), TestFusionCache.Create(), TestCachingOptions.Default());
        await StepstoneJobSearchTool.SearchStepstoneJobsAsync(service, search: "Go", countryCode: "DE");

        Job? job = await StepstoneJobByIdTool.GetStepstoneJobAsync(service, id: "51");

        Assert.NotNull(job);
        Assert.Equal("51", job.Id);
    }

    [Fact]
    public async Task GetStepstoneJobAsync_WithoutPriorSearch_ReturnsNull()
    {
        List<Job> jobs = Enumerable.Range(1, 51).Select(id => TestJobs.Create(id)).ToList();
        IStepstoneJobsService service = new StepstoneJobsService(
            new FakeJobsProvider(jobs), new JobSearchFilter(), TestFusionCache.Create(), TestCachingOptions.Default());

        Job? job = await StepstoneJobByIdTool.GetStepstoneJobAsync(service, id: "51");

        Assert.Null(job);
    }
}
