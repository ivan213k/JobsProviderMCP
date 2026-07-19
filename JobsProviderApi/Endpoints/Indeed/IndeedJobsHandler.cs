using Microsoft.AspNetCore.Mvc;
using JobsProviderApi.Models;
using JobsProviderApi.Services.Indeed;

namespace JobsProviderApi.Endpoints.Indeed;

internal static class IndeedJobsHandler
{
    public static async Task<IResult> HandleAsync(
        [AsParameters] JobSearchQuery query,
        IIndeedJobsService jobsService,
        CancellationToken cancellationToken)
    {
        var jobs = await jobsService.SearchAsync(query, cancellationToken);
        return Results.Ok(jobs);
    }
}
