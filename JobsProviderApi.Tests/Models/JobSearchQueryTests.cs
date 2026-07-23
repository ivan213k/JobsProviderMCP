using System.ComponentModel.DataAnnotations;
using JobsProviderApi.Models;

namespace JobsProviderApi.Tests.Models;

public class JobSearchQueryTests
{
    private static JobSearchQuery QueryWithSearch(string search) =>
        new(
            Search: search,
            MustHaveSkills: null,
            PreferredSkills: null,
            Locations: null,
            CountryCode: "DE");

    [Fact]
    public void Validate_WithSearchText_ReturnsNoErrors()
    {
        JobSearchQuery query = QueryWithSearch("Go Engineer");

        IEnumerable<ValidationResult> results = query.Validate(new ValidationContext(query));

        Assert.Empty(results);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Validate_WithBlankSearch_ReturnsErrorForSearch(string search)
    {
        JobSearchQuery query = QueryWithSearch(search);

        List<ValidationResult> results = query.Validate(new ValidationContext(query)).ToList();

        ValidationResult result = Assert.Single(results);
        Assert.Contains(nameof(JobSearchQuery.Search), result.MemberNames);
    }
}
