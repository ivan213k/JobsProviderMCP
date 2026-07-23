using Microsoft.AspNetCore.Mvc;
using JobsProviderApi.Models;
using JobsProviderApi.Services.Stepstone;

namespace JobsProviderApi.Endpoints.Stepstone;

internal static class StepstoneJobsHandler
{
    public static async Task<IResult> HandleAsync(
        [AsParameters] JobSearchQuery query,
        IStepstoneJobsService jobsService,
        CancellationToken cancellationToken)
    {
        ListResponse<Job> result = await jobsService.SearchAsync(query, cancellationToken);
        return Results.Ok(result);
    }
}
