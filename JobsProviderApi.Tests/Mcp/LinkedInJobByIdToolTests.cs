using JobsProviderApi.Mcp;
using JobsProviderApi.Models;
using JobsProviderApi.Services;
using JobsProviderApi.Services.LinkedIn;
using JobsProviderApi.Tests.Fakes;

namespace JobsProviderApi.Tests.Mcp;

public class LinkedInJobByIdToolTests
{
    [Fact]
    public async Task GetLinkedInJobAsync_AfterSearchReturnedTheJob_ReturnsIt()
    {
        var jobs = new[] { TestJobs.Create(1, title: "Senior Go Engineer") };
        ILinkedInJobsService service = new LinkedInJobsService(
            new FakeJobsProvider(jobs), new JobSearchFilter(), TestFusionCache.Create(), TestCachingOptions.Default());
        await LinkedInJobSearchTool.SearchLinkedInJobsAsync(service, search: "Go", countryCode: "DE");

        Job? job = await LinkedInJobByIdTool.GetLinkedInJobAsync(service, id: "1");

        Assert.NotNull(job);
        Assert.Equal("1", job.Id);
    }

    [Fact]
    public async Task GetLinkedInJobAsync_WithoutPriorSearch_ReturnsNull()
    {
        var jobs = new[] { TestJobs.Create(1, title: "Senior Go Engineer") };
        ILinkedInJobsService service = new LinkedInJobsService(
            new FakeJobsProvider(jobs), new JobSearchFilter(), TestFusionCache.Create(), TestCachingOptions.Default());

        Job? job = await LinkedInJobByIdTool.GetLinkedInJobAsync(service, id: "1");

        Assert.Null(job);
    }
}
