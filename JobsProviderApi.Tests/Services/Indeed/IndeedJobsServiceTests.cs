using JobsProviderApi.Models;
using JobsProviderApi.Services;
using JobsProviderApi.Services.Indeed;
using JobsProviderApi.Tests.Fakes;

namespace JobsProviderApi.Tests.Services.Indeed;

public class IndeedJobsServiceTests
{
    [Fact]
    public async Task SearchAsync_WithSameQueryTwice_OnlyFetchesJobsOnce()
    {
        var provider = new FakeJobsProvider([TestJobs.Create(1, title: "Go Engineer")]);
        IIndeedJobsService service = new IndeedJobsService(provider, new JobSearchFilter(), TestFusionCache.Create(), TestCachingOptions.Default());
        var query = new JobSearchQuery(Search: "Go", MustHaveSkills: null, PreferredSkills: null, Locations: null, CountryCode: "DE");

        await service.SearchAsync(query);
        await service.SearchAsync(query);

        Assert.Equal(1, provider.CallCount);
    }

    [Fact]
    public async Task SearchAsync_WithDifferentQueries_FetchesSeparately()
    {
        var provider = new FakeJobsProvider([TestJobs.Create(1, title: "Go Engineer"), TestJobs.Create(2, title: "Java Engineer")]);
        IIndeedJobsService service = new IndeedJobsService(provider, new JobSearchFilter(), TestFusionCache.Create(), TestCachingOptions.Default());

        await service.SearchAsync(new JobSearchQuery(Search: "Go", MustHaveSkills: null, PreferredSkills: null, Locations: null, CountryCode: "DE"));
        await service.SearchAsync(new JobSearchQuery(Search: "Java", MustHaveSkills: null, PreferredSkills: null, Locations: null, CountryCode: "DE"));

        Assert.Equal(2, provider.CallCount);
    }

    [Fact]
    public async Task GetByIdAsync_AfterSearchReturnedTheJob_ReturnsItFromCache()
    {
        var provider = new FakeJobsProvider([TestJobs.Create(1, title: "Go Engineer")]);
        IIndeedJobsService service = new IndeedJobsService(provider, new JobSearchFilter(), TestFusionCache.Create(), TestCachingOptions.Default());
        await service.SearchAsync(new JobSearchQuery(Search: "Go", MustHaveSkills: null, PreferredSkills: null, Locations: null, CountryCode: "DE"));

        Job? job = await service.GetByIdAsync("1");

        Assert.NotNull(job);
        Assert.Equal("1", job.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithoutPriorSearch_ReturnsNull()
    {
        var provider = new FakeJobsProvider([TestJobs.Create(1, title: "Go Engineer")]);
        IIndeedJobsService service = new IndeedJobsService(provider, new JobSearchFilter(), TestFusionCache.Create(), TestCachingOptions.Default());

        Job? job = await service.GetByIdAsync("1");

        Assert.Null(job);
    }
}
