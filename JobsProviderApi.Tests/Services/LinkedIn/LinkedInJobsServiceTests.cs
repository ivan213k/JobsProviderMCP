using ApifySdk.Actors.LinkedIn;
using ApifySdk.Actors.LinkedIn.Models;
using JobsProviderApi.Models;
using JobsProviderApi.Providers;
using JobsProviderApi.Services;
using JobsProviderApi.Services.LinkedIn;
using JobsProviderApi.Tests.Fakes;
using Moq;

namespace JobsProviderApi.Tests.Services.LinkedIn;

public class LinkedInJobsServiceTests
{
    [Fact]
    public async Task SearchAsync_WithSameQueryTwice_OnlyFetchesJobsOnce()
    {
        var provider = new FakeJobsProvider([TestJobs.Create(1, title: "Go Engineer")]);
        ILinkedInJobsService service = new LinkedInJobsService(provider, new JobSearchFilter(), TestFusionCache.Create(), TestCachingOptions.Default());
        var query = new JobSearchQuery(Search: "Go", MustHaveSkills: null, PreferredSkills: null, Locations: null, CountryCode: "DE");

        await service.SearchAsync(query);
        await service.SearchAsync(query);

        Assert.Equal(1, provider.CallCount);
    }

    [Fact]
    public async Task SearchAsync_WithDifferentQueries_FetchesSeparately()
    {
        var provider = new FakeJobsProvider([TestJobs.Create(1, title: "Go Engineer"), TestJobs.Create(2, title: "Java Engineer")]);
        ILinkedInJobsService service = new LinkedInJobsService(provider, new JobSearchFilter(), TestFusionCache.Create(), TestCachingOptions.Default());

        await service.SearchAsync(new JobSearchQuery(Search: "Go", MustHaveSkills: null, PreferredSkills: null, Locations: null, CountryCode: "DE"));
        await service.SearchAsync(new JobSearchQuery(Search: "Java", MustHaveSkills: null, PreferredSkills: null, Locations: null, CountryCode: "DE"));

        Assert.Equal(2, provider.CallCount);
    }

    [Fact]
    public async Task GetByIdAsync_AfterSearchReturnedTheJob_ReturnsItFromCache()
    {
        var provider = new FakeJobsProvider([TestJobs.Create(1, title: "Go Engineer")]);
        ILinkedInJobsService service = new LinkedInJobsService(provider, new JobSearchFilter(), TestFusionCache.Create(), TestCachingOptions.Default());
        await service.SearchAsync(new JobSearchQuery(Search: "Go", MustHaveSkills: null, PreferredSkills: null, Locations: null, CountryCode: "DE"));

        Job? job = await service.GetByIdAsync("1");

        Assert.NotNull(job);
        Assert.Equal("1", job.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithoutPriorSearch_ReturnsNull()
    {
        var provider = new FakeJobsProvider([TestJobs.Create(1, title: "Go Engineer")]);
        ILinkedInJobsService service = new LinkedInJobsService(provider, new JobSearchFilter(), TestFusionCache.Create(), TestCachingOptions.Default());

        Job? job = await service.GetByIdAsync("1");

        Assert.Null(job);
    }

    /// <summary>
    /// Pins the chain the whole source depends on: skills go out as resumeKeywords, the actor echoes the ones it
    /// found back as matchedKeywords, those become <see cref="Job.Requirements"/>, and
    /// <see cref="JobSearchFilter"/> matches them exactly. A change to either end silently returns zero results
    /// rather than failing, so it is asserted end to end.
    /// </summary>
    [Theory]
    [InlineData(new[] { "Go" }, new[] { "Go", "gRPC" }, true)]
    [InlineData(new[] { "go" }, new[] { "Go" }, true)]
    [InlineData(new[] { "Go", "gRPC" }, new[] { "Go" }, false)]
    [InlineData(new[] { "Kubernetes" }, new string[0], false)]
    public async Task SearchAsync_FiltersMustHaveSkillsAgainstMatchedKeywords(
        string[] mustHaveSkills,
        string[] matchedKeywords,
        bool expectedMatch)
    {
        ILinkedInJobsService service = CreateServiceOverActor(matchedKeywords);

        ListResponse<Job> result = await service.SearchAsync(new JobSearchQuery(
            Search: "backend",
            MustHaveSkills: mustHaveSkills,
            PreferredSkills: null,
            Locations: null,
            CountryCode: "DE"));

        Assert.Equal(expectedMatch ? 1 : 0, result.TotalCount);
    }

    [Theory]
    [InlineData(new[] { "Go", "Rust" }, new[] { "Go" }, true)]
    [InlineData(new[] { "Go", "Rust" }, new string[0], false)]
    public async Task SearchAsync_FiltersPreferredSkillsAgainstMatchedKeywords(
        string[] preferredSkills,
        string[] matchedKeywords,
        bool expectedMatch)
    {
        ILinkedInJobsService service = CreateServiceOverActor(matchedKeywords);

        ListResponse<Job> result = await service.SearchAsync(new JobSearchQuery(
            Search: "backend",
            MustHaveSkills: null,
            PreferredSkills: preferredSkills,
            Locations: null,
            CountryCode: "DE"));

        Assert.Equal(expectedMatch ? 1 : 0, result.TotalCount);
    }

    private static ILinkedInJobsService CreateServiceOverActor(string[] matchedKeywords)
    {
        var actor = new Mock<ILinkedInActor>(MockBehavior.Strict);
        actor
            .Setup(a => a.SearchAsync(It.IsAny<LinkedInSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new LinkedInJobResult
                {
                    JobId = "abc123",
                    JobTitle = "Backend Engineer",
                    JobUrl = "https://linkedin.com/jobs/view/abc123",
                    PublishedAt = new DateTime(2026, 1, 1),
                    Location = "Berlin, Germany",
                    CompanyName = "Acme",
                    JobDescription = "Build services.",
                    MatchedKeywords = matchedKeywords,
                }
            ]);

        return new LinkedInJobsService(
            new LinkedInJobsProvider(actor.Object, TestFusionCache.Create()),
            new JobSearchFilter(),
            TestFusionCache.Create(),
            TestCachingOptions.Default());
    }
}
