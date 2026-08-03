using System.ComponentModel.DataAnnotations;
using JobsProviderApi.Models;

namespace JobsProviderApi.Tests.Models;

/// <summary>
/// Both `GET /api/indeed/jobs` and `GET /api/stepstone/jobs` bind `[AsParameters] JobSearchQuery` and share the
/// same `AddValidation()` endpoint filter, so validating the model here covers the `countryCode` check for both.
/// </summary>
public class JobSearchQueryValidationTests
{
    [Theory]
    [InlineData("XX1")]
    [InlineData("1")]
    [InlineData("DEU")]
    [InlineData("")]
    public void Validate_WithInvalidCountryCode_ReturnsValidationError(string countryCode)
    {
        var query = new JobSearchQuery(Search: "engineer", SearchAliases: null, MustHaveSkills: null, PreferredSkills: null, Locations: null, CountryCode: countryCode);

        List<ValidationResult> results = Validate(query);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(JobSearchQuery.CountryCode)));
    }

    [Fact]
    public void Validate_WithValidCountryCode_ReturnsNoValidationError()
    {
        var query = new JobSearchQuery(Search: "engineer", SearchAliases: null, MustHaveSkills: null, PreferredSkills: null, Locations: null, CountryCode: "DE");

        List<ValidationResult> results = Validate(query);

        Assert.DoesNotContain(results, result => result.MemberNames.Contains(nameof(JobSearchQuery.CountryCode)));
    }

    private static List<ValidationResult> Validate(JobSearchQuery query)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(query, new ValidationContext(query), results, validateAllProperties: true);
        return results;
    }
}
