using System.Collections.Immutable;
using ApifySdk.Actors.Indeed;
using ApifySdk.Actors.Indeed.Models;
using JobsProviderApi.Models;
using System.Globalization;
using System.Text.Json;
using ZiggyCreatures.Caching.Fusion;

namespace JobsProviderApi.Providers;

public class IndeedJobsProvider(IIndeedActor indeedActor, IFusionCache cache) : IIndeedJobsProvider
{
    public const string MaxAgeOfPostingInDays = "14";

    public const int Limit = 300;

    private const string CacheKeySource = "indeed:provider";

    public async Task<IReadOnlyList<Job>> GetJobsAsync(JobSearchQuery query, CancellationToken cancellationToken = default)
    {
        IndeedSearchRequest request = ToSearchRequest(query, query.Search);
        IReadOnlyList<Job> jobs = await GetOrFetchJobsAsync(request, cancellationToken);

        string[] aliases = NormalizeAliases(query.SearchAliases);
        if (jobs.Count >= Limit || aliases.Length == 0)
            return jobs;

        return await WidenWithAliasesAsync(jobs, query, aliases, cancellationToken);
    }
    
    private async Task<IReadOnlyList<Job>> WidenWithAliasesAsync(
        IEnumerable<Job> jobs,
        JobSearchQuery query,
        string[] aliases,
        CancellationToken cancellationToken)
    {
        Dictionary<string, Job> jobsById = ToJobsById(jobs);

        foreach (string alias in aliases)
        {
            foreach (Job job in await GetOrFetchJobsAsync(ToSearchRequest(query, alias), cancellationToken))
            {
                jobsById.TryAdd(job.Id, job);
            }

            if (jobsById.Count >= Limit)
                break;
        }

        return jobsById.Values.ToImmutableList();
    }

    private async Task<IReadOnlyList<Job>> GetOrFetchJobsAsync(IndeedSearchRequest request, CancellationToken cancellationToken) =>
        await cache.GetOrSetAsync(
            ToCacheKey(request),
            ct => FetchJobsAsync(request, ct),
            token: cancellationToken);

    private static Dictionary<string, Job> ToJobsById(IEnumerable<Job> jobs)
    {
        Dictionary<string, Job> jobsById = [];
        foreach (Job job in jobs)
        {
            jobsById.TryAdd(job.Id, job);
        }

        return jobsById;
    }

    private static string[] NormalizeAliases(IReadOnlyList<string>? searchAliases) =>
        searchAliases?
            .Where(alias => !string.IsNullOrWhiteSpace(alias))
            .Select(alias => alias.Trim())
            .ToArray() ?? [];

    private async Task<IReadOnlyList<Job>> FetchJobsAsync(IndeedSearchRequest request, CancellationToken cancellationToken)
    {
        IEnumerable<IndeedJobResult> results = await indeedActor.SearchAsync(request, cancellationToken);

        return results
            .Select(result => ToJob(result))
            .ToImmutableList();
    }

    private static string ToCacheKey(IndeedSearchRequest request) =>
        $"{CacheKeySource}:{JsonSerializer.Serialize(request)}";

    private IndeedSearchRequest ToSearchRequest(JobSearchQuery query, string search) =>
        new()
        {
            Search = search,
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
