using System.Collections.Immutable;
using ApifySdk.Actors.Indeed;
using ApifySdk.Actors.Indeed.Models;
using JobsProviderApi.Models;
using System.Globalization;

namespace JobsProviderApi.Providers;

public class IndeedJobsProvider(IIndeedActor indeedActor) : IIndeedJobsProvider
{
    public const string MaxAgeOfPostingInDays = "14";

    public const int Limit = 100;

    public async Task<IReadOnlyList<Job>> GetJobsAsync(JobSearchQuery query, CancellationToken cancellationToken = default)
    {
        IndeedSearchRequest request = ToSearchRequest(query);

        IEnumerable<IndeedJobResult> results = await indeedActor.SearchAsync(request, cancellationToken);

        return results
            .Select(result => ToJob(result))
            .ToImmutableList();
    }
    
    private IndeedSearchRequest ToSearchRequest(JobSearchQuery query) =>
        new()
        {
            Keywords = ToKeywords(query.Search, query.MustHaveSkills),
            Location = ToSingleOrEmptySearchLocation(query.Locations),
            Country = query.CountryCode.ToLowerInvariant(),
            MaxAgeOfPostingInDays = MaxAgeOfPostingInDays,
            Limit = Limit
        };

    private string ToSingleOrEmptySearchLocation(IReadOnlyList<string>? locations)
    {
        if (locations is not null && locations.Count == 1)
            return locations[0];

        return string.Empty;
    }

    private string ToKeywords(string search, IReadOnlyList<string>? requirements)
    {
        if (requirements is null)
            return search;

        List<string> keywords = [search];

        foreach (string requirement in requirements)
        {
            if (!string.IsNullOrWhiteSpace(requirement))
            {
                keywords.Add(requirement);
            }
        }

        return string.Join(' ', keywords);
    }

    private Job ToJob(IndeedJobResult result)
    {
        IReadOnlyList<string>? requirements = result.Attributes?.Values.ToImmutableList();

        return new Job(
            Id: result.Key,
            Title: result.Title,
            Company: result.Employer?.Name,
            Location: FormatLocation(result.Location),
            Description: result.Description.Text,
            Requirements: requirements,
            Link: result.Url,
            SourcingPlatform: "Indeed",
            DatePublished: result.DatePublished.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }

    private string? FormatLocation(JobLocation? location)
    {
        string?[] locationParts = [location?.City, location?.CountryName];
        string joinedLocation = string.Join(", ", locationParts.Where(part => !string.IsNullOrWhiteSpace(part)));
        return string.IsNullOrEmpty(joinedLocation) ? null : joinedLocation;
    }
}
