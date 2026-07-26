using System.ComponentModel;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace JobsProviderApi.Models;

public static class JobSearchQueryDescriptions
{
    public const string Search =
        "Space-separated keywords. A job matches when AT LEAST ONE keyword appears in its title or description " +
        "(this is not a required exact phrase). Prefer specific, distinctive keywords and avoid common ones like " +
        "`developer` or `engineer` that match almost every posting.";
    public const string MustHaveSkills = "Skills that must ALL be present among a job's requirements for it to be included.";
    public const string PreferredSkills = "Skills where at least ONE must be present among a job's requirements for it to be included.";
    public const string Locations = "Locations where at least ONE must match a job's location for it to be included.";
    public const string CountryCode = "ISO 3166-1 alpha-2 country code (e.g. `DE`) identifying which regional job board to search.";
    public const string Skip = "Number of matching jobs to skip before applying `take`, for pagination. Defaults to 0.";
    public const string Take = "Maximum number of jobs to return after `skip` is applied. Defaults to 100.";
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

    [property: FromQuery(Name = "skip")]
    [property: Description(JobSearchQueryDescriptions.Skip)]
    int Skip = 0,

    [property: FromQuery(Name = "take")]
    [property: Description(JobSearchQueryDescriptions.Take)]
    int Take = 100)
{
    public string ToCacheKey(string source) => $"{source}:{JsonSerializer.Serialize(this)}";
}
