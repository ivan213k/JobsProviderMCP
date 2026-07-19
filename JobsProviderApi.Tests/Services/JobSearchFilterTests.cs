using JobsProviderApi.Models;
using JobsProviderApi.Services;

namespace JobsProviderApi.Tests.Services;

public class JobSearchFilterTests
{
    private static readonly JobSearchQuery MatchAllQuery = new(".*", MustHaveSkills: null, PreferredSkills: null, PreferredLocations: null, "DE", Take: 1000);
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

        IReadOnlyList<Job> result = _sut.Apply(jobs, MatchAllQuery with { Search = "Go" });

        Assert.Equal([1, 2], result.Select(j => j.Id));
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

        IReadOnlyList<Job> result = _sut.Apply(jobs, MatchAllQuery with { MustHaveSkills = ["Go", "gRPC"] });

        Assert.Equal([1], result.Select(j => j.Id));
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

        IReadOnlyList<Job> result = _sut.Apply(jobs, MatchAllQuery with { PreferredSkills = ["Rust", "Ruby"] });

        Assert.Equal([1, 2], result.Select(j => j.Id));
    }

    [Fact]
    public void Apply_SkillMatching_IsExactNotSubstring()
    {
        var jobs = new[]
        {
            TestJobs.Create(1, requirements: ["Go"]),
            TestJobs.Create(2, requirements: ["MongoDB"]),
        };

        IReadOnlyList<Job> result = _sut.Apply(jobs, MatchAllQuery with { MustHaveSkills = ["Go"] });

        Assert.Equal([1], result.Select(j => j.Id));
    }

    [Fact]
    public void Apply_SkillMatching_IsCaseInsensitive()
    {
        var jobs = new[] { TestJobs.Create(1, requirements: ["Go"]) };

        IReadOnlyList<Job> result = _sut.Apply(jobs, MatchAllQuery with { MustHaveSkills = ["go"] });

        Assert.Equal([1], result.Select(j => j.Id));
    }

    [Fact]
    public void Apply_WithPreferredLocations_RequiresAtLeastOneLocationToMatch()
    {
        var jobs = new[]
        {
            TestJobs.Create(1) with { Location = "Berlin, Germany (Hybrid)" },
            TestJobs.Create(2) with { Location = "Remote (EU)" },
            TestJobs.Create(3) with { Location = "Austin, TX (On-site)" },
        };

        IReadOnlyList<Job> result = _sut.Apply(jobs, MatchAllQuery with { PreferredLocations = ["Berlin", "Remote"] });

        Assert.Equal([1, 2], result.Select(j => j.Id));
    }

    [Fact]
    public void Apply_LocationMatching_IsSubstringNotExactlyLocation()
    {
        var jobs = new[]
        {
            TestJobs.Create(1) with { Location = "Berlin, Germany (Hybrid)" },
            TestJobs.Create(2) with { Location = "Austin, TX (On-site)" },
        };

        IReadOnlyList<Job> result = _sut.Apply(jobs, MatchAllQuery with { PreferredLocations = ["germany"] });

        Assert.Equal([1], result.Select(j => j.Id));
    }

    [Fact]
    public void Apply_CombinesFiltersWithAnd()
    {
        var jobs = new[]
        {
            TestJobs.Create(1, title: "Senior Go Engineer", requirements: ["Go", "gRPC"]),
            TestJobs.Create(2, title: "Senior Go Engineer", requirements: ["Go"]),
            TestJobs.Create(3, title: "Java Engineer", requirements: ["Go", "gRPC"]),
        };

        IReadOnlyList<Job> result = _sut.Apply(
            jobs,
            new JobSearchQuery("Go Engineer", ["Go", "gRPC"], PreferredSkills: null, PreferredLocations: null, "DE"));

        Assert.Equal([1], result.Select(j => j.Id));
    }

    [Fact]
    public void Apply_WithoutTake_DefaultsToTen()
    {
        List<Job> jobs = Enumerable.Range(1, 20).Select(id => TestJobs.Create(id)).ToList();

        IReadOnlyList<Job> result = _sut.Apply(
            jobs,
            new JobSearchQuery(".*", MustHaveSkills: null, PreferredSkills: null, PreferredLocations: null, "DE"));

        Assert.Equal(10, result.Count);
        Assert.Equal(Enumerable.Range(1, 10), result.Select(j => j.Id));
    }

    [Fact]
    public void Apply_WithTake_LimitsResults()
    {
        List<Job> jobs = Enumerable.Range(1, 20).Select(id => TestJobs.Create(id)).ToList();

        IReadOnlyList<Job> result = _sut.Apply(jobs, MatchAllQuery with { Take = 3 });

        Assert.Equal([1, 2, 3], result.Select(j => j.Id));
    }

    [Fact]
    public void Apply_WithMustHaveSkills_ExcludesJobsWithNullRequirements()
    {
        var jobs = new[]
        {
            TestJobs.Create(1) with { Requirements = null },
            TestJobs.Create(2, requirements: ["Go"]),
        };

        IReadOnlyList<Job> result = _sut.Apply(jobs, MatchAllQuery with { MustHaveSkills = ["Go"] });

        Assert.Equal([2], result.Select(j => j.Id));
    }

    [Fact]
    public void Apply_WithPreferredSkills_ExcludesJobsWithNullRequirements()
    {
        var jobs = new[]
        {
            TestJobs.Create(1) with { Requirements = null },
            TestJobs.Create(2, requirements: ["Go"]),
        };

        IReadOnlyList<Job> result = _sut.Apply(jobs, MatchAllQuery with { PreferredSkills = ["Go"] });

        Assert.Equal([2], result.Select(j => j.Id));
    }
}
