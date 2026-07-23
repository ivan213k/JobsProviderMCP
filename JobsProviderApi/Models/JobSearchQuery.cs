using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace JobsProviderApi.Models;

public record JobSearchQuery(
    [property: FromQuery(Name = "search")]
    [property: Description("Plain text matched (case-insensitively) as a substring of a job's title or description.")]
    string Search,

    [property: FromQuery(Name = "mustHaveSkills")]
    [property: Description("Skills that must ALL be present among a job's requirements for it to be included.")]
    string[]? MustHaveSkills,

    [property: FromQuery(Name = "preferredSkills")]
    [property: Description("Skills where at least ONE must be present among a job's requirements for it to be included.")]
    string[]? PreferredSkills,

    [property: FromQuery(Name = "locations")]
    [property: Description("Locations where at least ONE must match a job's location for it to be included.")]
    string[]? Locations,

    [property: FromQuery(Name = "countryCode")]
    [property: Description("ISO 3166-1 alpha-2 country code (e.g. `DE`) identifying which regional job board to search.")]
    string CountryCode,

    [property: FromQuery(Name = "take")]
    [property: Description("Maximum number of jobs to return. Defaults to 100.")]
    int Take = 100) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Search))
        {
            yield return new ValidationResult("'search' must not be blank.", [nameof(Search)]);
        }
    }
}
