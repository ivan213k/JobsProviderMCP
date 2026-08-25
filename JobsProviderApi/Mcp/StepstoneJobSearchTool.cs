using System.ComponentModel;
using JobsProviderApi.Models;
using JobsProviderApi.Services.Stepstone;
using ModelContextProtocol.Server;

namespace JobsProviderApi.Mcp;

[McpServerToolType]
public static class StepstoneJobSearchTool
{
    [McpServerTool(Name = "search_stepstone_jobs")]
    [Description("""
        Search Stepstone job postings. `search` and `countryCode` are required; the rest are optional and combined
        using AND. Results are sorted newest-first by date published.
        """)]
    public static async Task<ListResponse<Job>> SearchStepstoneJobsAsync(
        IStepstoneJobsService jobsService,
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
