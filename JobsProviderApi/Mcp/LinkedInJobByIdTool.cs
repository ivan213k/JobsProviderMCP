using System.ComponentModel;
using JobsProviderApi.Models;
using JobsProviderApi.Services.LinkedIn;
using ModelContextProtocol.Server;

namespace JobsProviderApi.Mcp;

[McpServerToolType]
public static class LinkedInJobByIdTool
{
    [McpServerTool(Name = "get_linkedin_job")]
    [Description("""
        Get a single LinkedIn job posting by id. Cache-only: only returns a job that has previously been returned
        by search_linkedin_jobs and whose individual cache entry hasn't expired; returns null otherwise, with no
        fallback fetch. Because `requirements` on this source reflects the skills of the search that cached the
        job, the value returned here is the one from whichever search last returned it.
        """)]
    public static async Task<Job?> GetLinkedInJobAsync(
        ILinkedInJobsService jobsService,
        [Description("The job's id.")] string id,
        CancellationToken cancellationToken = default) =>
        await jobsService.GetByIdAsync(id, cancellationToken);
}
