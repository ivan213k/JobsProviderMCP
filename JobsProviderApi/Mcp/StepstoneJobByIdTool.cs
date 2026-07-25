using System.ComponentModel;
using JobsProviderApi.Models;
using JobsProviderApi.Services.Stepstone;
using ModelContextProtocol.Server;

namespace JobsProviderApi.Mcp;

[McpServerToolType]
public static class StepstoneJobByIdTool
{
    [McpServerTool(Name = "get_stepstone_job")]
    [Description("""
        Get a single Stepstone job posting by id. Cache-only: only returns a job that has previously been
        returned by search_stepstone_jobs and whose individual cache entry hasn't expired; returns null
        otherwise, with no fallback fetch.
        """)]
    public static async Task<Job?> GetStepstoneJobAsync(
        IStepstoneJobsService jobsService,
        [Description("The job's id.")] string id,
        CancellationToken cancellationToken = default) =>
        await jobsService.GetByIdAsync(id, cancellationToken);
}
