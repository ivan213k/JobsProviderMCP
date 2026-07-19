using System.ComponentModel.DataAnnotations;
using JobsProviderApi.Models;

namespace JobsProviderApi.Tests.Models;

public class JobSearchQueryTests
{
    [Fact]
    public void Validate_WithValidRegex_ReturnsNoErrors()
    {
        var query = new JobSearchQuery("Go|Java", null, null);

        IEnumerable<ValidationResult> results = query.Validate(new ValidationContext(query));

        Assert.Empty(results);
    }

    [Fact]
    public void Validate_WithInvalidRegex_ReturnsErrorForSearch()
    {
        var query = new JobSearchQuery("[", null, null);

        List<ValidationResult> results = query.Validate(new ValidationContext(query)).ToList();

        ValidationResult result = Assert.Single(results);
        Assert.Contains(nameof(JobSearchQuery.Search), result.MemberNames);
    }
}
