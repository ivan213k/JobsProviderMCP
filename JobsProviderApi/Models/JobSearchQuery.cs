using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace JobsProviderApi.Models;

public static class JobSearchQueryDescriptions
{
    public const string Search =
        "Case-insensitive plain-text match against each job's title or description. " +
        "Prefer a single keyword (`C#`) for a wide result pool, or plain text (`Junior C# Developer`) " +
        "for the most relevant results.";
    public const string SearchAliases =
        "Fallback search terms, strongest first — ordering matters, so put the closest equivalents at the " +
        "front to avoid running short of results. Used only when `search` alone returns fewer results than " +
        "requested. Examples: `.NET` as an alias for `C#`; `C# Entwickler` as a localised alias for " +
        "`C# Developer`.";
    public const string MustHaveSkills = "Skills that must ALL be present among a job's requirements for it to be included.";
    public const string PreferredSkills = "Skills where at least ONE must be present among a job's requirements for it to be included.";
    public const string Locations = "Locations where at least ONE must match a job's location for it to be included.";
    public const string CountryCode = "ISO 3166-1 alpha-2 country code (e.g. `DE`) identifying which regional job board to search.";
    public const string CountryCodePattern = "^[A-Za-z]{2}$";
    public const string CountryCodeValidationError = "countryCode must be a 2-letter ISO 3166-1 alpha-2 code (e.g. 'DE').";
    public const string Skip = "Number of matching jobs to skip before applying `take`, for pagination. Defaults to 0.";
    public const string Take = "Maximum number of jobs to return after `skip` is applied. Defaults to 100.";
}

public record JobSearchQuery(
    [property: FromQuery(Name = "search")]
    [property: Description(JobSearchQueryDescriptions.Search)]
    string Search,

    [property: FromQuery(Name = "searchAliases")]
    [property: Description(JobSearchQueryDescriptions.SearchAliases)]
    string[]? SearchAliases,

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
    [property: Required(AllowEmptyStrings = false, ErrorMessage = JobSearchQueryDescriptions.CountryCodeValidationError)]
    [property: RegularExpression(JobSearchQueryDescriptions.CountryCodePattern, ErrorMessage = JobSearchQueryDescriptions.CountryCodeValidationError)]
    string CountryCode,

    [property: FromQuery(Name = "skip")]
    [property: Description(JobSearchQueryDescriptions.Skip)]
    int Skip = 0,

    [property: FromQuery(Name = "take")]
    [property: Description(JobSearchQueryDescriptions.Take)]
    int Take = 100)
{
    public string ToCacheKey(string source) => $"{source}:{JsonSerializer.Serialize(this)}";
}
