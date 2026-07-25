using System.Collections.Immutable;
using ApifySdk.Actors.Indeed;
using ApifySdk.Actors.Indeed.Models;
using JobsProviderApi.Models;

namespace JobsProviderApi.Providers;

public class IndeedJobsProvider(IIndeedActor indeedActor) : IIndeedJobsProvider
{
    private const string MaxAgeOfPostingInDays = "7";

    private const int Limit = 10;

    public async Task<IReadOnlyList<Job>> GetJobsAsync(JobSearchQuery query, CancellationToken cancellationToken = default)
    {
        IndeedSearchRequest request = ToSearchRequest(query);

        IEnumerable<IndeedJobResult> results = await indeedActor.SearchAsync(request, cancellationToken);

        return results
            .Select((result, index) => ToJob(result, index))
            .ToImmutableList();
    }

    private static IndeedSearchRequest ToSearchRequest(JobSearchQuery query) =>
        new()
        {
            Keywords = ToKeywords(query.Search, query.MustHaveSkills),
            Location = query.Locations is [var location] ? location : string.Empty,
            Country = query.CountryCode.ToLowerInvariant(),
            MaxAgeOfPostingInDays = MaxAgeOfPostingInDays,
            Limit = Limit
        };

    private static string ToKeywords(string search, IReadOnlyList<string>? requirements)
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

    private static Job ToJob(IndeedJobResult result, int index)
    {
        IReadOnlyList<string>? requirements = result.Attributes?.Values.ToImmutableList();

        return new Job(
            Id: index,
            Title: result.Title,
            Company: result.Employer?.Name ?? string.Empty,
            Location: FormatLocation(result.Location),
            Description: result.Description.Text,
            Requirements: requirements,
            Link: result.Url,
            SourcingPlatform: "Indeed",
            DatePublished: FormatDatePublished(result.DatePublished));
    }

    private static string FormatLocation(JobLocation? location)
    {
        if (location is null)
        {
            return string.Empty;
        }

        string?[] locationParts = [location.City, location.CountryName];
        return string.Join(',', locationParts.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private static string FormatDatePublished(DateTime? datePublished) =>
        datePublished?.ToString("yyyy-MM-dd") ?? string.Empty;
}
