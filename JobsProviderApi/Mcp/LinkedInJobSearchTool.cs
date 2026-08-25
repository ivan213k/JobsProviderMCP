using System.ComponentModel;
using JobsProviderApi.Models;
using JobsProviderApi.Services.LinkedIn;
using ModelContextProtocol.Server;

namespace JobsProviderApi.Mcp;

[McpServerToolType]
public static class LinkedInJobSearchTool
{
    [McpServerTool(Name = "search_linkedin_jobs")]
    [Description("""
        Search LinkedIn job postings. `search` and `countryCode` are required; the rest are optional and combined
        using AND. Results are sorted newest-first by date published. This source has no requirements field of
        its own, so `requirements` on each job lists the `mustHaveSkills`/`preferredSkills` from this request that
        were found in the posting, rather than everything the posting asks for; matching is literal, so
        `Kubernetes` will not match a posting that only says `K8s`. `countryCode` is used as the search location
        only when no `locations` are given.
        """)]
    public static async Task<ListResponse<Job>> SearchLinkedInJobsAsync(
        ILinkedInJobsService jobsService,
        [Description(JobSearchQueryDescriptions.Search)] string search,
        [Description(JobSearchQueryDescriptions.CountryCode)] string countryCode,
        [Description(JobSearchQueryDescriptions.SearchAliases)] string[]? searchAliases = null,
        [Description(JobSearchQueryDescriptions.MustHaveSkills)] string[]? mustHaveSkills = null,
        [Description(JobSearchQueryDescriptions.PreferredSkills)] string[]? preferredSkills = null,
        [Description(JobSearchQueryDescriptions.Locations)] string[]? locations = null,
        [Description(JobSearchQueryDescriptions.Skip)] int skip = 0,
        [Description(JobSearchQueryDescriptions.Take)] int take = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new JobSearchQuery(search, searchAliases, mustHaveSkills, preferredSkills, locations, countryCode, skip, take);
        return await jobsService.SearchAsync(query, cancellationToken);
    }
}
