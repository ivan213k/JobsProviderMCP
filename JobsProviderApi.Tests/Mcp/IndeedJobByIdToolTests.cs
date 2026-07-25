using JobsProviderApi.Mcp;
using JobsProviderApi.Models;
using JobsProviderApi.Services;
using JobsProviderApi.Services.Indeed;
using JobsProviderApi.Tests.Fakes;

namespace JobsProviderApi.Tests.Mcp;

public class IndeedJobByIdToolTests
{
    [Fact]
    public async Task GetIndeedJobAsync_AfterSearchReturnedTheJob_ReturnsIt()
    {
        var jobs = new[] { TestJobs.Create(1, title: "Senior Go Engineer") };
        IIndeedJobsService service = new IndeedJobsService(
            new FakeJobsProvider(jobs), new JobSearchFilter(), TestFusionCache.Create(), TestCachingOptions.Default());
        await IndeedJobSearchTool.SearchIndeedJobsAsync(service, search: "Go", countryCode: "DE");

        Job? job = await IndeedJobByIdTool.GetIndeedJobAsync(service, id: "1");

        Assert.NotNull(job);
        Assert.Equal("1", job.Id);
    }

    [Fact]
    public async Task GetIndeedJobAsync_WithoutPriorSearch_ReturnsNull()
    {
        var jobs = new[] { TestJobs.Create(1, title: "Senior Go Engineer") };
        IIndeedJobsService service = new IndeedJobsService(
            new FakeJobsProvider(jobs), new JobSearchFilter(), TestFusionCache.Create(), TestCachingOptions.Default());

        Job? job = await IndeedJobByIdTool.GetIndeedJobAsync(service, id: "1");

        Assert.Null(job);
    }
}
