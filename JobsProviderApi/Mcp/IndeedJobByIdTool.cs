using System.ComponentModel;
using JobsProviderApi.Models;
using JobsProviderApi.Services.Indeed;
using ModelContextProtocol.Server;

namespace JobsProviderApi.Mcp;

[McpServerToolType]
public static class IndeedJobByIdTool
{
    [McpServerTool(Name = "get_indeed_job")]
    [Description("""
        Get a single Indeed job posting by id. Cache-only: only returns a job that has previously been returned
        by search_indeed_jobs and whose individual cache entry hasn't expired; returns null otherwise, with no
        fallback fetch.
        """)]
    public static async Task<Job?> GetIndeedJobAsync(
        IIndeedJobsService jobsService,
        [Description("The job's id.")] string id,
        CancellationToken cancellationToken = default) =>
        await jobsService.GetByIdAsync(id, cancellationToken);
}
