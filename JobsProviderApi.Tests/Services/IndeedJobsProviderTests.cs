using ApifySdk.Actors.Indeed;
using ApifySdk.Actors.Indeed.Models;
using JobsProviderApi.Models;
using JobsProviderApi.Providers;
using Moq;

namespace JobsProviderApi.Tests.Services;

public class IndeedJobsProviderTests
{
    private static readonly JobSearchQuery MatchAllQuery = new(
        Search: "backend",
        MustHaveSkills: null,
        PreferredSkills: null,
        Locations: null,
        CountryCode: "DE");

    private readonly Mock<IIndeedActor> _actor = new(MockBehavior.Strict);

    private IndeedJobsProvider CreateSut(params IndeedJobResult[] results)
    {
        _actor
            .Setup(actor => actor.SearchAsync(It.IsAny<IndeedSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(results);

        return new IndeedJobsProvider(_actor.Object);
    }

    [Fact]
    public async Task GetJobsAsync_MapsAllFields()
    {
        IndeedJobResult result = new()
        {
            Key = "abc123",
            Title = "Senior Go Engineer",
            Url = "https://indeed.com/jobs/abc123",
            DatePublished = new DateTime(2026, 1, 15),
            Location = new JobLocation { City = "Berlin", CountryName = "Germany" },
            Employer = new JobEmployer { Name = "Acme" },
            Description = new JobDescription { Text = "Build services." },
            Attributes = new Dictionary<string, string> { ["s1"] = "Go", ["s2"] = "gRPC" },
        };
        IndeedJobsProvider sut = CreateSut(result);

        Job job = Assert.Single(await sut.GetJobsAsync(MatchAllQuery));

        Assert.Equal("abc123", job.Id);
        Assert.Equal("Senior Go Engineer", job.Title);
        Assert.Equal("Acme", job.Company);
        Assert.Equal("Berlin, Germany", job.Location);
        Assert.Equal("Build services.", job.Description);
        Assert.Equal(["Go", "gRPC"], job.Requirements);
        Assert.Equal("https://indeed.com/jobs/abc123", job.Link);
        Assert.Equal("Indeed", job.SourcingPlatform);
        Assert.Equal("2026-01-15", job.DatePublished);
    }

    [Fact]
    public async Task GetJobsAsync_WithoutEmployer_MapsCompanyToNull()
    {
        IndeedJobResult result = CreateResult();
        result.Employer = null;
        IndeedJobsProvider sut = CreateSut(result);

        Job job = Assert.Single(await sut.GetJobsAsync(MatchAllQuery));

        Assert.Null(job.Company);
    }

    [Fact]
    public async Task GetJobsAsync_WithoutAttributes_MapsRequirementsToNull()
    {
        IndeedJobResult result = CreateResult();
        result.Attributes = null;
        IndeedJobsProvider sut = CreateSut(result);

        Job job = Assert.Single(await sut.GetJobsAsync(MatchAllQuery));

        Assert.Null(job.Requirements);
    }

    [Theory]
    [InlineData("Berlin", "Germany", "Berlin, Germany")]
    [InlineData("Berlin", null, "Berlin")]
    [InlineData(null, "Germany", "Germany")]
    [InlineData(" ", "Germany", "Germany")]
    public async Task GetJobsAsync_FormatsLocationFromCityAndCountry(string? city, string? country, string expected)
    {
        IndeedJobResult result = CreateResult();
        result.Location = new JobLocation { City = city, CountryName = country };
        IndeedJobsProvider sut = CreateSut(result);

        Job job = Assert.Single(await sut.GetJobsAsync(MatchAllQuery));

        Assert.Equal(expected, job.Location);
    }

    [Fact]
    public async Task GetJobsAsync_WithoutLocation_MapsLocationToNull()
    {
        IndeedJobResult result = CreateResult();
        result.Location = null;
        IndeedJobsProvider sut = CreateSut(result);

        Job job = Assert.Single(await sut.GetJobsAsync(MatchAllQuery));

        Assert.Null(job.Location);
    }

    [Fact]
    public async Task GetJobsAsync_WithEmptyLocationParts_MapsLocationToNull()
    {
        IndeedJobResult result = CreateResult();
        result.Location = new JobLocation { City = null, CountryName = " " };
        IndeedJobsProvider sut = CreateSut(result);

        Job job = Assert.Single(await sut.GetJobsAsync(MatchAllQuery));

        Assert.Null(job.Location);
    }

    private static IndeedJobResult CreateResult() =>
        new()
        {
            Key = "key",
            Title = "Backend Engineer",
            Url = "https://indeed.com/jobs/key",
            DatePublished = new DateTime(2026, 1, 1),
            Location = new JobLocation { City = "Berlin", CountryName = "Germany" },
            Employer = new JobEmployer { Name = "Acme" },
            Description = new JobDescription { Text = "General description." },
            Attributes = new Dictionary<string, string> { ["s1"] = "Go" },
        };
}
