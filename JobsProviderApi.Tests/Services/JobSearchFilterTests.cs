using JobsProviderApi.Models;
using JobsProviderApi.Services;

namespace JobsProviderApi.Tests.Services;

public class JobSearchFilterTests
{
    private static readonly JobSearchQuery MatchAllQuery = new(".*", null, null, Take: 1000);
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

        var result = _sut.Apply(jobs, MatchAllQuery with { Search = "Go" });

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

        var result = _sut.Apply(jobs, MatchAllQuery with { MustHaveSkills = ["Go", "gRPC"] });

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

        var result = _sut.Apply(jobs, MatchAllQuery with { PreferredSkills = ["Rust", "Ruby"] });

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

        var result = _sut.Apply(jobs, MatchAllQuery with { MustHaveSkills = ["Go"] });

        Assert.Equal([1], result.Select(j => j.Id));
    }

    [Fact]
    public void Apply_SkillMatching_IsCaseInsensitive()
    {
        var jobs = new[] { TestJobs.Create(1, requirements: ["Go"]) };

        var result = _sut.Apply(jobs, MatchAllQuery with { MustHaveSkills = ["go"] });

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

        var result = _sut.Apply(jobs, new JobSearchQuery("Go Engineer", ["Go", "gRPC"], null));

        Assert.Equal([1], result.Select(j => j.Id));
    }

    [Fact]
    public void Apply_WithoutTake_DefaultsToTen()
    {
        var jobs = Enumerable.Range(1, 20).Select(id => TestJobs.Create(id)).ToList();

        var result = _sut.Apply(jobs, new JobSearchQuery(".*", null, null));

        Assert.Equal(10, result.Count);
        Assert.Equal(Enumerable.Range(1, 10), result.Select(j => j.Id));
    }

    [Fact]
    public void Apply_WithTake_LimitsResults()
    {
        var jobs = Enumerable.Range(1, 20).Select(id => TestJobs.Create(id)).ToList();

        var result = _sut.Apply(jobs, MatchAllQuery with { Take = 3 });

        Assert.Equal([1, 2, 3], result.Select(j => j.Id));
    }
}
