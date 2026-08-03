using System.Collections.Immutable;
using ApifySdk.Actors.LinkedIn;
using ApifySdk.Actors.LinkedIn.Models;
using JobsProviderApi.Models;
using System.Globalization;
using System.Text.Json;
using ZiggyCreatures.Caching.Fusion;

namespace JobsProviderApi.Providers;

/// <summary>
/// Serves LinkedIn jobs from the Apify actor. The actor exposes no requirements/skills field, so the query's
/// must-have and preferred skills are sent as the actor's <c>resumeKeywords</c> and the <c>matchedKeywords</c>
/// that come back become <see cref="Job.Requirements"/> — which is what lets
/// <see cref="Services.JobSearchFilter"/> apply the skill filters unchanged.
/// </summary>
public class LinkedInJobsProvider(ILinkedInActor linkedInActor, IFusionCache cache) : ILinkedInJobsProvider
{
    /// <summary>How far back postings are searched; see <see cref="Published"/> for the values the actor allows.</summary>
    public const Published MaxAgeOfPosting = Published.Past30Days;

    /// <summary>requires at least 150 results per run.</summary>
    public const int MaxItems = 150;

    private const string CacheKeySource = "linkedin:provider";

    public async Task<IReadOnlyList<Job>> GetJobsAsync(JobSearchQuery query, CancellationToken cancellationToken = default)
    {
        LinkedInSearchRequest request = ToSearchRequest(query);

        return await cache.GetOrSetAsync(
            ToCacheKey(request),
            ct => FetchJobsAsync(request, ct),
            token: cancellationToken);
    }

    private async Task<IReadOnlyList<Job>> FetchJobsAsync(LinkedInSearchRequest request, CancellationToken cancellationToken)
    {
        IEnumerable<LinkedInJobResult> results = await linkedInActor.SearchAsync(request, cancellationToken);

        return results
            .Select(result => ToJob(result))
            .ToImmutableList();
    }

    private static string ToCacheKey(LinkedInSearchRequest request) =>
        $"{CacheKeySource}:{JsonSerializer.Serialize(request)}";

    private LinkedInSearchRequest ToSearchRequest(JobSearchQuery query) =>
        new()
        {
            Keywords = ToKeywords(query.Search, query.SearchAliases),
            Locations = ToSearchLocations(query.Locations, query.CountryCode),
            ResumeKeywords = ToResumeKeywords(query.MustHaveSkills, query.PreferredSkills),
            PublishedAt = MaxAgeOfPosting,
            MaxItems = MaxItems,
            SaveOnlyUniqueItems = true
        };

    private static string[] ToKeywords(string search, IReadOnlyList<string>? searchAliases) =>
        new[] { search }
            .Concat(NormalizeAliases(searchAliases))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static string[] NormalizeAliases(IReadOnlyList<string>? searchAliases) =>
        searchAliases?
            .Where(alias => !string.IsNullOrWhiteSpace(alias))
            .Select(alias => alias.Trim())
            .ToArray() ?? [];

    private string[] ToSearchLocations(IReadOnlyList<string>? locations, string countryCode)
    {
        string[] requestedLocations = locations?
            .Where(location => !string.IsNullOrWhiteSpace(location))
            .Select(location => location.Trim())
            .ToArray() ?? [];

        if (requestedLocations.Length > 0)
            return requestedLocations;

        string? countryName = ToCountryName(countryCode);
        return countryName is null ? [] : [countryName];
    }

    private string? ToCountryName(string countryCode)
    {
        try
        {
            return new RegionInfo(countryCode).EnglishName;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private ResumeKeyword[] ToResumeKeywords(IReadOnlyList<string>? mustHaveSkills, IReadOnlyList<string>? preferredSkills) =>
        (mustHaveSkills ?? [])
            .Concat(preferredSkills ?? [])
            .Where(skill => !string.IsNullOrWhiteSpace(skill))
            .Select(skill => skill.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(skill => new ResumeKeyword { Keyword = skill })
            .ToArray();

    private Job ToJob(LinkedInJobResult result) =>
        new(
            Id: result.JobId,
            Title: result.JobTitle,
            Company: result.CompanyName,
            Location: result.Location,
            Description: result.JobDescription,
            Requirements: result.MatchedKeywords?.ToImmutableList(),
            Link: result.JobUrl,
            SourcingPlatform: "LinkedIn",
            DatePublished: result.PublishedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
}
