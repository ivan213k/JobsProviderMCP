using System.Text.Json;
using ApifySdk.Actors.LinkedIn;
using ApifySdk.Actors.LinkedIn.Models;
using JobsProviderApi.Models;
using JobsProviderApi.Providers;
using JobsProviderApi.Tests.Fakes;
using Moq;

namespace JobsProviderApi.Tests.Services;

public class LinkedInJobsProviderTests
{
    private static readonly JobSearchQuery MatchAllQuery = new(
        Search: "backend",
        SearchAliases: null,
        MustHaveSkills: null,
        PreferredSkills: null,
        Locations: null,
        CountryCode: "DE");

    private readonly Mock<ILinkedInActor> _actor = new(MockBehavior.Strict);

    private LinkedInJobsProvider CreateSut(params LinkedInJobResult[] results)
    {
        _actor
            .Setup(actor => actor.SearchAsync(It.IsAny<LinkedInSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(results);

        return new LinkedInJobsProvider(_actor.Object, TestFusionCache.Create());
    }

    [Fact]
    public async Task GetJobsAsync_MapsAllFields()
    {
        LinkedInJobResult result = new()
        {
            JobId = "abc123",
            JobTitle = "Senior Go Engineer",
            JobUrl = "https://linkedin.com/jobs/view/abc123",
            PublishedAt = new DateTime(2026, 1, 15),
            Location = "Berlin, Germany",
            CompanyName = "Acme",
            JobDescription = "Build services.",
            MatchedKeywords = ["Go", "gRPC"],
        };
        LinkedInJobsProvider sut = CreateSut(result);

        Job job = Assert.Single(await sut.GetJobsAsync(MatchAllQuery));

        Assert.Equal("abc123", job.Id);
        Assert.Equal("Senior Go Engineer", job.Title);
        Assert.Equal("Acme", job.Company);
        Assert.Equal("Berlin, Germany", job.Location);
        Assert.Equal("Build services.", job.Description);
        Assert.Equal(["Go", "gRPC"], job.Requirements);
        Assert.Equal("https://linkedin.com/jobs/view/abc123", job.Link);
        Assert.Equal("LinkedIn", job.SourcingPlatform);
        Assert.Equal("2026-01-15", job.DatePublished);
    }

    [Fact]
    public async Task GetJobsAsync_WithoutCompanyName_MapsCompanyToNull()
    {
        LinkedInJobResult result = CreateResult();
        result.CompanyName = null;
        LinkedInJobsProvider sut = CreateSut(result);

        Job job = Assert.Single(await sut.GetJobsAsync(MatchAllQuery));

        Assert.Null(job.Company);
    }

    [Fact]
    public async Task GetJobsAsync_WithoutMatchedKeywords_MapsRequirementsToNull()
    {
        LinkedInJobResult result = CreateResult();
        result.MatchedKeywords = null;
        LinkedInJobsProvider sut = CreateSut(result);

        Job job = Assert.Single(await sut.GetJobsAsync(MatchAllQuery));

        Assert.Null(job.Requirements);
    }

    [Fact]
    public async Task GetJobsAsync_BuildsSearchRequestFromQuery()
    {
        LinkedInJobsProvider sut = CreateSut();

        await sut.GetJobsAsync(new JobSearchQuery(
            Search: "backend",
            SearchAliases: null,
            MustHaveSkills: ["Go"],
            PreferredSkills: ["gRPC"],
            Locations: ["Berlin"],
            CountryCode: "DE"));

        _actor.Verify(
            actor => actor.SearchAsync(
                It.Is<LinkedInSearchRequest>(request =>
                    request.Keywords.SequenceEqual(new[] { "backend" })
                    && request.Locations.SequenceEqual(new[] { "Berlin" })
                    && request.ResumeKeywords.Select(keyword => keyword.Keyword).SequenceEqual(new[] { "Go", "gRPC" })
                    && request.PublishedAt == LinkedInJobsProvider.MaxAgeOfPosting
                    && request.MaxItems == LinkedInJobsProvider.MaxItems
                    && request.SaveOnlyUniqueItems),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(null, new[] { "backend" })]
    [InlineData(new string[0], new[] { "backend" })]
    [InlineData(new[] { "back-end" }, new[] { "backend", "back-end" })]
    [InlineData(new[] { "back-end", "serverside" }, new[] { "backend", "back-end", "serverside" })]
    [InlineData(new[] { " back-end ", "   " }, new[] { "backend", "back-end" })]
    [InlineData(new[] { "BACKEND", "back-end" }, new[] { "backend", "back-end" })]
    public async Task GetJobsAsync_SendsSearchAndAliasesAsKeywords(string[]? searchAliases, string[] expectedKeywords)
    {
        LinkedInJobsProvider sut = CreateSut();

        await sut.GetJobsAsync(MatchAllQuery with { SearchAliases = searchAliases });

        _actor.Verify(
            actor => actor.SearchAsync(
                It.Is<LinkedInSearchRequest>(request => request.Keywords.SequenceEqual(expectedKeywords)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetJobsAsync_WithAliases_StillOnlyCallsTheActorOnce()
    {
        LinkedInJobsProvider sut = CreateSut();

        await sut.GetJobsAsync(MatchAllQuery with { SearchAliases = ["back-end", "serverside"] });

        _actor.Verify(
            actor => actor.SearchAsync(It.IsAny<LinkedInSearchRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetJobsAsync_DoesNotFoldSkillsIntoSearchKeywords()
    {
        LinkedInJobsProvider sut = CreateSut();

        await sut.GetJobsAsync(MatchAllQuery with { MustHaveSkills = ["Go", "gRPC"] });

        _actor.Verify(
            actor => actor.SearchAsync(
                It.Is<LinkedInSearchRequest>(request => request.Keywords.SequenceEqual(new[] { "backend" })),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(null, null, new string[0])]
    [InlineData(new[] { "Go" }, null, new[] { "Go" })]
    [InlineData(null, new[] { "gRPC" }, new[] { "gRPC" })]
    [InlineData(new[] { "Go", "  " }, new[] { "gRPC" }, new[] { "Go", "gRPC" })]
    [InlineData(new[] { " Go " }, null, new[] { "Go" })]
    [InlineData(new[] { "Go" }, new[] { "go", "gRPC" }, new[] { "Go", "gRPC" })]
    public async Task GetJobsAsync_SendsSkillsAsDedupedResumeKeywords(
        string[]? mustHaveSkills,
        string[]? preferredSkills,
        string[] expectedKeywords)
    {
        LinkedInJobsProvider sut = CreateSut();

        await sut.GetJobsAsync(MatchAllQuery with { MustHaveSkills = mustHaveSkills, PreferredSkills = preferredSkills });

        _actor.Verify(
            actor => actor.SearchAsync(
                It.Is<LinkedInSearchRequest>(request =>
                    request.ResumeKeywords.Select(keyword => keyword.Keyword).SequenceEqual(expectedKeywords)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Aliases are unused for now, but the actor rejects the whole run if <c>aliases</c> is null rather than an
    /// empty array, so "no aliases" has to mean empty and not missing.
    /// </summary>
    [Fact]
    public async Task GetJobsAsync_SendsResumeKeywordsWithEmptyAliases()
    {
        LinkedInJobsProvider sut = CreateSut();

        await sut.GetJobsAsync(MatchAllQuery with { MustHaveSkills = ["Kubernetes"] });

        _actor.Verify(
            actor => actor.SearchAsync(
                It.Is<LinkedInSearchRequest>(request =>
                    request.ResumeKeywords.All(keyword => keyword.Aliases != null && keyword.Aliases.Length == 0)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(new[] { "Berlin" }, new[] { "Berlin" })]
    [InlineData(new[] { "Berlin", "Munich" }, new[] { "Berlin", "Munich" })]
    [InlineData(new[] { " Berlin ", "  " }, new[] { "Berlin" })]
    public async Task GetJobsAsync_PassesRequestedLocationsThrough(string[] locations, string[] expectedLocations)
    {
        LinkedInJobsProvider sut = CreateSut();

        await sut.GetJobsAsync(MatchAllQuery with { Locations = locations });

        _actor.Verify(
            actor => actor.SearchAsync(
                It.Is<LinkedInSearchRequest>(request => request.Locations.SequenceEqual(expectedLocations)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("DE", new[] { "Germany" })]
    [InlineData("de", new[] { "Germany" })]
    [InlineData("XX", new string[0])]
    public async Task GetJobsAsync_WithoutLocations_FallsBackToCountryName(string countryCode, string[] expectedLocations)
    {
        LinkedInJobsProvider sut = CreateSut();

        await sut.GetJobsAsync(MatchAllQuery with { Locations = null, CountryCode = countryCode });

        _actor.Verify(
            actor => actor.SearchAsync(
                It.Is<LinkedInSearchRequest>(request => request.Locations.SequenceEqual(expectedLocations)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// The enum is only useful if it serializes to the exact strings the actor accepts — anything else fails the
    /// run as a billed 400, the same way a null <c>aliases</c> did, so the wire values are pinned not trusted.
    /// </summary>
    [Theory]
    [InlineData(Published.Past24Hours, "r86400")]
    [InlineData(Published.Past7Days, "r604800")]
    [InlineData(Published.Past30Days, "r2592000")]
    public void Published_SerializesToActorWireValue(Published published, string expectedWireValue)
    {
        Assert.Equal($"\"{expectedWireValue}\"", JsonSerializer.Serialize(published));
    }

    private static LinkedInJobResult CreateResult() =>
        new()
        {
            JobId = "key",
            JobTitle = "Backend Engineer",
            JobUrl = "https://linkedin.com/jobs/view/key",
            PublishedAt = new DateTime(2026, 1, 1),
            Location = "Berlin, Germany",
            CompanyName = "Acme",
            JobDescription = "General description.",
            MatchedKeywords = ["Go"],
        };
}
