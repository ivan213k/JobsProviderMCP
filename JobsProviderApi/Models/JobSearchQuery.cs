using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace JobsProviderApi.Models;

public static class JobSearchQueryDescriptions
{
    public const string Search = "Regular expression matched (case-insensitive) against each job's title or description.";
    public const string MustHaveSkills = "Skills that must ALL be present among a job's requirements for it to be included.";
    public const string PreferredSkills = "Skills where at least ONE must be present among a job's requirements for it to be included.";
    public const string Locations = "Locations where at least ONE must match a job's location for it to be included.";
    public const string CountryCode = "ISO 3166-1 alpha-2 country code (e.g. `DE`) identifying which regional job board to search.";
    public const string Take = "Maximum number of jobs to return. Defaults to 10.";
}

public record JobSearchQuery(
    [property: FromQuery(Name = "search")]
    [property: Description(JobSearchQueryDescriptions.Search)]
    string Search,

    [property: FromQuery(Name = "mustHaveSkills")]
    [property: Description(JobSearchQueryDescriptions.MustHaveSkills)]
    string[]? MustHaveSkills,

    [property: FromQuery(Name = "preferredSkills")]
    [property: Description(JobSearchQueryDescriptions.PreferredSkills)]
    string[]? PreferredSkills,

    [property: FromQuery(Name = "locations")]
    [property: Description(JobSearchQueryDescriptions.Locations)]
    string[]? Locations,

    [property: FromQuery(Name = "countryCode")]
    [property: Description(JobSearchQueryDescriptions.CountryCode)]
    string CountryCode,

    [property: FromQuery(Name = "take")]
    [property: Description(JobSearchQueryDescriptions.Take)]
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
