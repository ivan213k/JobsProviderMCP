using JobsProviderApi.Mcp;
using JobsProviderApi.Models;
using JobsProviderApi.Services;
using JobsProviderApi.Services.Indeed;
using JobsProviderApi.Tests.Fakes;
using ModelContextProtocol;

namespace JobsProviderApi.Tests.Mcp;

public class IndeedJobSearchToolTests
{
    [Fact]
    public async Task SearchIndeedJobsAsync_WithValidQuery_ReturnsFilteredJobs()
    {
        var jobs = new[]
        {
            TestJobs.Create(1, title: "Senior Go Engineer"),
            TestJobs.Create(2, title: "Java Engineer"),
        };
        IIndeedJobsService service = new IndeedJobsService(new FakeJobsProvider(jobs), new JobSearchFilter());

        IReadOnlyList<Job> result = await IndeedJobSearchTool.SearchIndeedJobsAsync(
            service,
            search: "Go",
            mustHaveSkills: null,
            preferredSkills: null,
            locations: null,
            countryCode: "DE");

        Assert.Equal([1], result.Select(j => j.Id));
    }

    [Fact]
    public async Task SearchIndeedJobsAsync_WithInvalidRegex_ThrowsMcpException()
    {
        IIndeedJobsService service = new IndeedJobsService(new FakeJobsProvider([]), new JobSearchFilter());

        await Assert.ThrowsAsync<McpException>(() => IndeedJobSearchTool.SearchIndeedJobsAsync(
            service,
            search: "[",
            mustHaveSkills: null,
            preferredSkills: null,
            locations: null,
            countryCode: "DE"));
    }
}
