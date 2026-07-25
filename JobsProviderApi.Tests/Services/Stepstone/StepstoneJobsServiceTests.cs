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
        IStepstoneJobsService service = new StepstoneJobsService(provider, new JobSearchFilter(), TestFusionCache.Create());
        var query = new JobSearchQuery(Search: "Go", MustHaveSkills: null, PreferredSkills: null, Locations: null, CountryCode: "DE");

        await service.SearchAsync(query);
        await service.SearchAsync(query);

        Assert.Equal(1, provider.CallCount);
    }

    [Fact]
    public async Task SearchAsync_WithDifferentQueries_FetchesSeparately()
    {
        var provider = new FakeJobsProvider([TestJobs.Create(1, title: "Go Engineer"), TestJobs.Create(2, title: "Java Engineer")]);
        IStepstoneJobsService service = new StepstoneJobsService(provider, new JobSearchFilter(), TestFusionCache.Create());

        await service.SearchAsync(new JobSearchQuery(Search: "Go", MustHaveSkills: null, PreferredSkills: null, Locations: null, CountryCode: "DE"));
        await service.SearchAsync(new JobSearchQuery(Search: "Java", MustHaveSkills: null, PreferredSkills: null, Locations: null, CountryCode: "DE"));

        Assert.Equal(2, provider.CallCount);
    }
}
