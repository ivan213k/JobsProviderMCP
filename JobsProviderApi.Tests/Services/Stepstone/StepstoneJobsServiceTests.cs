using JobsProviderApi.Models;
using JobsProviderApi.Services;
using JobsProviderApi.Services.Stepstone;
using JobsProviderApi.Tests.Fakes;

namespace JobsProviderApi.Tests.Services.Stepstone;

public class StepstoneJobsServiceTests
{
    [Fact]
    public async Task SearchAsync_WithSameQueryTwice_OnlyFetchesJobsOnce()
    {
        var provider = new FakeJobsProvider([TestJobs.Create(1, title: "Go Engineer")]);
        IStepstoneJobsService service = new StepstoneJobsService(provider, new JobSearchFilter(), TestFusionCache.Create(), TestCachingOptions.Default());
        var query = new JobSearchQuery(Search: "Go", MustHaveSkills: null, PreferredSkills: null, Locations: null, CountryCode: "DE");

        await service.SearchAsync(query);
        await service.SearchAsync(query);

        Assert.Equal(1, provider.CallCount);
    }

    [Fact]
    public async Task SearchAsync_WithDifferentQueries_FetchesSeparately()
    {
        var provider = new FakeJobsProvider([TestJobs.Create(1, title: "Go Engineer"), TestJobs.Create(2, title: "Java Engineer")]);
        IStepstoneJobsService service = new StepstoneJobsService(provider, new JobSearchFilter(), TestFusionCache.Create(), TestCachingOptions.Default());

        await service.SearchAsync(new JobSearchQuery(Search: "Go", MustHaveSkills: null, PreferredSkills: null, Locations: null, CountryCode: "DE"));
        await service.SearchAsync(new JobSearchQuery(Search: "Java", MustHaveSkills: null, PreferredSkills: null, Locations: null, CountryCode: "DE"));

        Assert.Equal(2, provider.CallCount);
    }

    [Fact]
    public async Task GetByIdAsync_AfterSearchReturnedTheJob_ReturnsItFromCache()
    {
        // StepstoneJobsService skips the first 50 jobs, so the fake provider needs at least that many filler jobs
        // ahead of the one under test.
        List<Job> jobs = Enumerable.Range(1, 50).Select(id => TestJobs.Create(id)).ToList();
        jobs.Add(TestJobs.Create(51, title: "Go Engineer"));
        var provider = new FakeJobsProvider(jobs);
        IStepstoneJobsService service = new StepstoneJobsService(provider, new JobSearchFilter(), TestFusionCache.Create(), TestCachingOptions.Default());
        await service.SearchAsync(new JobSearchQuery(Search: "Go", MustHaveSkills: null, PreferredSkills: null, Locations: null, CountryCode: "DE"));

        Job? job = await service.GetByIdAsync("51");

        Assert.NotNull(job);
        Assert.Equal("51", job.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithoutPriorSearch_ReturnsNull()
    {
        List<Job> jobs = Enumerable.Range(1, 51).Select(id => TestJobs.Create(id)).ToList();
        var provider = new FakeJobsProvider(jobs);
        IStepstoneJobsService service = new StepstoneJobsService(provider, new JobSearchFilter(), TestFusionCache.Create(), TestCachingOptions.Default());

        Job? job = await service.GetByIdAsync("51");

        Assert.Null(job);
    }
}
