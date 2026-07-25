using JobsProviderApi.Models;
using JobsProviderApi.Services;

namespace JobsProviderApi.Tests.Services;

public class JobSearchFilterTests
{
    private static readonly JobSearchQuery MatchAllQuery = new(
        Search: "",
        MustHaveSkills: null,
        PreferredSkills: null,
        Locations: null,
        CountryCode: "DE",
        Take: 1000);

    private readonly JobSearchFilter _sut = new();

    [Fact]
    public void Apply_WithSearch_MatchesTitleOrDescription()
    {
        var jobs = new[]
        {
            TestJobs.Create(1, title: "Senior Go Engineer"),
            TestJobs.Create(2, title: "Frontend Engineer", description: "Loves Go tooling."),
            TestJobs.Create(3, title: "Java Engineer", description: "No relevant tech."),
        };

        ListResponse<Job> result = _sut.Apply(jobs, MatchAllQuery with { Search = "Go" });

        Assert.Equal(["1", "2"], result.Items.Select(j => j.Id));
    }

    [Fact]
    public void Apply_SearchMatching_IsCaseInsensitive()
    {
        var jobs = new[] { TestJobs.Create(1, title: "Senior GO Engineer") };

        ListResponse<Job> result = _sut.Apply(jobs, MatchAllQuery with { Search = "go" });

        Assert.Equal(["1"], result.Items.Select(j => j.Id));
    }

    [Fact]
    public void Apply_WithMustHaveSkills_RequiresAllSkillsPresent()
    {
        var jobs = new[]
        {
            TestJobs.Create(1, requirements: ["Go", "gRPC", "PostgreSQL"]),
            TestJobs.Create(2, requirements: ["Go"]),
            TestJobs.Create(3, requirements: ["Java"]),
        };

        ListResponse<Job> result = _sut.Apply(jobs, MatchAllQuery with { MustHaveSkills = ["Go", "gRPC"] });

        Assert.Equal(["1"], result.Items.Select(j => j.Id));
    }

    [Fact]
    public void Apply_WithPreferredSkills_RequiresAtLeastOneSkillPresent()
    {
        var jobs = new[]
        {
            TestJobs.Create(1, requirements: ["Rust"]),
            TestJobs.Create(2, requirements: ["Ruby"]),
            TestJobs.Create(3, requirements: ["Java"]),
        };

        ListResponse<Job> result = _sut.Apply(jobs, MatchAllQuery with { PreferredSkills = ["Rust", "Ruby"] });

        Assert.Equal(["1", "2"], result.Items.Select(j => j.Id));
    }

    [Fact]
    public void Apply_SkillMatching_IsExactNotSubstring()
    {
        var jobs = new[]
        {
            TestJobs.Create(1, requirements: ["Go"]),
            TestJobs.Create(2, requirements: ["MongoDB"]),
        };

        ListResponse<Job> result = _sut.Apply(jobs, MatchAllQuery with { MustHaveSkills = ["Go"] });

        Assert.Equal(["1"], result.Items.Select(j => j.Id));
    }

    [Fact]
    public void Apply_SkillMatching_IsCaseInsensitive()
    {
        var jobs = new[] { TestJobs.Create(1, requirements: ["Go"]) };

        ListResponse<Job> result = _sut.Apply(jobs, MatchAllQuery with { MustHaveSkills = ["go"] });

        Assert.Equal(["1"], result.Items.Select(j => j.Id));
    }

    [Fact]
    public void Apply_LocationMatching_ReturnsJobsMatchingAnyRequestedLocation()
    {
        var jobs = new[]
        {
            TestJobs.Create(1) with { Location = "Berlin, Germany (Hybrid)" },
            TestJobs.Create(2) with { Location = "Remote (EU)" },
            TestJobs.Create(3) with { Location = "Austin, TX (On-site)" },
        };

        ListResponse<Job> result = _sut.Apply(jobs, MatchAllQuery with { Locations = ["Berlin", "Remote"] });

        Assert.Equal(["1", "2"], result.Items.Select(j => j.Id));
    }

    [Fact]
    public void Apply_LocationMatching_IsSubstringNotExactlyLocation()
    {
        var jobs = new[]
        {
            TestJobs.Create(1) with { Location = "Berlin, Germany (Hybrid)" },
            TestJobs.Create(2) with { Location = "Austin, TX (On-site)" },
        };

        ListResponse<Job> result = _sut.Apply(jobs, MatchAllQuery with { Locations = ["germany"] });

        Assert.Equal(["1"], result.Items.Select(j => j.Id));
    }

    [Fact]
    public void Apply_CombinesFiltersWithAnd()
    {
        var jobs = new[]
        {
            TestJobs.Create(1, title: "Senior Go Engineer", requirements: ["Go", "gRPC", "Kubernetes"])
                with { Location = "Berlin, Germany (Hybrid)" },
            TestJobs.Create(2, title: "Java Engineer", requirements: ["Go", "gRPC", "Kubernetes"])
                with { Location = "Berlin, Germany (Hybrid)" },
            TestJobs.Create(3, title: "Senior Go Engineer", requirements: ["Go", "Kubernetes"])
                with { Location = "Berlin, Germany (Hybrid)" },
            TestJobs.Create(4, title: "Senior Go Engineer", requirements: ["Go", "gRPC"])
                with { Location = "Berlin, Germany (Hybrid)" },
            TestJobs.Create(5, title: "Senior Go Engineer", requirements: ["Go", "gRPC", "Kubernetes"])
                with { Location = "Austin, TX (On-site)" },
        };

        ListResponse<Job> result = _sut.Apply(
            jobs,
            new JobSearchQuery(
                Search: "Go Engineer",
                MustHaveSkills: ["Go", "gRPC"],
                PreferredSkills: ["Kubernetes", "Terraform"],
                Locations: ["Berlin"],
                CountryCode: "DE"));

        Assert.Equal(["1"], result.Items.Select(j => j.Id));
    }

    [Fact]
    public void Apply_WithoutTake_DefaultsToOneHundred()
    {
        List<Job> jobs = Enumerable.Range(1, 150).Select(id => TestJobs.Create(id)).ToList();

        ListResponse<Job> result = _sut.Apply(
            jobs,
            new JobSearchQuery(
                Search: "",
                MustHaveSkills: null,
                PreferredSkills: null,
                Locations: null,
                CountryCode: "DE"));

        Assert.Equal(100, result.Items.Count);
        Assert.Equal(Enumerable.Range(1, 100).Select(id => id.ToString()), result.Items.Select(j => j.Id));
    }

    [Fact]
    public void Apply_WithTake_LimitsResults()
    {
        List<Job> jobs = Enumerable.Range(1, 20).Select(id => TestJobs.Create(id)).ToList();

        ListResponse<Job> result = _sut.Apply(jobs, MatchAllQuery with { Take = 3 });

        Assert.Equal(["1", "2", "3"], result.Items.Select(j => j.Id));
    }

    [Fact]
    public void Apply_WithoutSkip_DefaultsToZero()
    {
        List<Job> jobs = Enumerable.Range(1, 5).Select(id => TestJobs.Create(id)).ToList();
        ListResponse<Job> result = _sut.Apply(jobs, MatchAllQuery with { Take = 5 });
        Assert.Equal(["1", "2", "3", "4", "5"], result.Items.Select(j => j.Id));
    }

    [Fact]
    public void Apply_WithSkip_SkipsResultsAfterFiltering()
    {
        List<Job> jobs = Enumerable.Range(1, 5).Select(id => TestJobs.Create(id)).ToList();
        ListResponse<Job> result = _sut.Apply(jobs, MatchAllQuery with { Skip = 2, Take = 5 });
        Assert.Equal(["3", "4", "5"], result.Items.Select(j => j.Id));
    }

    [Fact]
    public void Apply_TotalCount_ReflectsMatchesBeforeSkipAndTake()
    {
        List<Job> jobs = Enumerable.Range(1, 20).Select(id => TestJobs.Create(id)).ToList();
        ListResponse<Job> result = _sut.Apply(jobs, MatchAllQuery with { Skip = 5, Take = 3 });

        Assert.Equal(20, result.TotalCount);
        Assert.Equal(3, result.Items.Count);
    }

    [Fact]
    public void Apply_TotalCount_ReflectsOnlyFilterMatches()
    {
        var jobs = new[]
        {
            TestJobs.Create(1, requirements: ["Go"]),
            TestJobs.Create(2, requirements: ["Java"]),
            TestJobs.Create(3, requirements: ["Go"]),
        };

        ListResponse<Job> result = _sut.Apply(jobs, MatchAllQuery with { MustHaveSkills = ["Go"] });

        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public void Apply_WithMustHaveSkills_ExcludesJobsWithNullRequirements()
    {
        var jobs = new[]
        {
            TestJobs.Create(1) with { Requirements = null },
            TestJobs.Create(2, requirements: ["Go"]),
        };

        ListResponse<Job> result = _sut.Apply(jobs, MatchAllQuery with { MustHaveSkills = ["Go"] });

        Assert.Equal(["2"], result.Items.Select(j => j.Id));
    }

    [Fact]
    public void Apply_WithPreferredSkills_ExcludesJobsWithNullRequirements()
    {
        var jobs = new[]
        {
            TestJobs.Create(1) with { Requirements = null },
            TestJobs.Create(2, requirements: ["Go"]),
        };

        ListResponse<Job> result = _sut.Apply(jobs, MatchAllQuery with { PreferredSkills = ["Go"] });

        Assert.Equal(["2"], result.Items.Select(j => j.Id));
    }
}
