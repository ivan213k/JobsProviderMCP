using JobsProviderApi.Mcp;
using JobsProviderApi.Models;
using JobsProviderApi.Services;
using JobsProviderApi.Services.LinkedIn;
using JobsProviderApi.Tests.Fakes;

namespace JobsProviderApi.Tests.Mcp;

public class LinkedInJobSearchToolTests
{
    [Fact]
    public async Task SearchLinkedInJobsAsync_WithMustHaveSkills_FiltersOnRequirements()
    {
        ILinkedInJobsService service = new LinkedInJobsService(
            new FakeJobsProvider([
                TestJobs.Create(1, "Backend Engineer", "General description.", "Go"),
                TestJobs.Create(2, "Backend Engineer", "General description.", "Java")
            ]),
            new JobSearchFilter(),
            TestFusionCache.Create(), TestCachingOptions.Default());

        ListResponse<Job> result = await LinkedInJobSearchTool.SearchLinkedInJobsAsync(
            service,
            search: "Backend",
            countryCode: "DE",
            mustHaveSkills: ["Go"]);

        Assert.Equal(["1"], result.Items.Select(j => j.Id));
    }

    [Fact]
    public async Task SearchLinkedInJobsAsync_WithSkip_SkipsResults()
    {
        List<Job> jobs = Enumerable.Range(1, 5).Select(id => TestJobs.Create(id)).ToList();
        ILinkedInJobsService service = new LinkedInJobsService(new FakeJobsProvider(jobs), new JobSearchFilter(), TestFusionCache.Create(), TestCachingOptions.Default());

        ListResponse<Job> result = await LinkedInJobSearchTool.SearchLinkedInJobsAsync(
            service,
            search: "",
            countryCode: "DE",
            skip: 3);

        Assert.Equal(5, result.TotalCount);
        Assert.Equal(["4", "5"], result.Items.Select(j => j.Id));
    }
}
