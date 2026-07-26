using JobsProviderApi.Mcp;
using JobsProviderApi.Models;
using JobsProviderApi.Services;
using JobsProviderApi.Services.Indeed;
using JobsProviderApi.Tests.Fakes;

namespace JobsProviderApi.Tests.Mcp;

public class IndeedJobSearchToolTests
{
    [Fact]
    public async Task SearchIndeedJobsAsync_WithSpecialCharacters_MatchesAsPlainText()
    {
        IIndeedJobsService service = new IndeedJobsService(
            new FakeJobsProvider([TestJobs.Create(1, title: "Backend Engineer (C++/Go)")]),
            new JobSearchFilter(),
            TestFusionCache.Create(), TestCachingOptions.Default());

        ListResponse<Job> result = await IndeedJobSearchTool.SearchIndeedJobsAsync(
            service,
            search: "(C++/Go)",
            countryCode: "DE");

        Assert.Equal(["1"], result.Items.Select(j => j.Id));
    }

    [Fact]
    public async Task SearchIndeedJobsAsync_WithSkip_SkipsResults()
    {
        List<Job> jobs = Enumerable.Range(1, 5).Select(id => TestJobs.Create(id)).ToList();
        IIndeedJobsService service = new IndeedJobsService(new FakeJobsProvider(jobs), new JobSearchFilter(), TestFusionCache.Create(), TestCachingOptions.Default());

        ListResponse<Job> result = await IndeedJobSearchTool.SearchIndeedJobsAsync(
            service,
            search: "",
            countryCode: "DE",
            skip: 3);

        Assert.Equal(5, result.TotalCount);
        Assert.Equal(["4", "5"], result.Items.Select(j => j.Id));
    }
}
