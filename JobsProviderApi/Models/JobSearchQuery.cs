using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace JobsProviderApi.Models;

public record JobSearchQuery(
    [property: FromQuery(Name = "search")]
    [property: Description("Regular expression matched (case-insensitive) against each job's title or description.")]
    string Search,

    [property: FromQuery(Name = "mustHaveSkills")]
    [property: Description("Skills that must ALL be present among a job's requirements for it to be included.")]
    string[]? MustHaveSkills,

    [property: FromQuery(Name = "preferredSkills")]
    [property: Description("Skills where at least ONE must be present among a job's requirements for it to be included.")]
    string[]? PreferredSkills,

    [property: FromQuery(Name = "preferredLocations")]
    [property: Description("Locations where at least ONE must match a job's location for it to be included.")]
    string[]? PreferredLocations,

    [property: FromQuery(Name = "countryCode")]
    [property: Description("ISO 3166-1 alpha-2 country code (e.g. `DE`) identifying which regional job board to search.")]
    string CountryCode,

    [property: FromQuery(Name = "take")]
    [property: Description("Maximum number of jobs to return. Defaults to 10.")]
    int Take = 10) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        string? error = null;
        try
        {
            _ = new Regex(Search);
        }
        catch (ArgumentException ex)
        {
            error = $"'{Search}' is not a valid regular expression: {ex.Message}";
        }

        if (error is not null)
        {
            yield return new ValidationResult(error, [nameof(Search)]);
        }
    }
}
