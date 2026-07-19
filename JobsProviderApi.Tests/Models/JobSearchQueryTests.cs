using System.ComponentModel.DataAnnotations;
using JobsProviderApi.Models;

namespace JobsProviderApi.Tests.Models;

public class JobSearchQueryTests
{
    [Fact]
    public void Validate_WithValidRegex_ReturnsNoErrors()
    {
        var query = new JobSearchQuery("Go|Java", null, null);

        var results = query.Validate(new ValidationContext(query));

        Assert.Empty(results);
    }

    [Fact]
    public void Validate_WithInvalidRegex_ReturnsErrorForSearch()
    {
        var query = new JobSearchQuery("[", null, null);

        var results = query.Validate(new ValidationContext(query)).ToList();

        var result = Assert.Single(results);
        Assert.Contains(nameof(JobSearchQuery.Search), result.MemberNames);
    }
}
