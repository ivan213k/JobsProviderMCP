using Microsoft.AspNetCore.Mvc;
using JobsProviderApi.Models;
using JobsProviderApi.Services.LinkedIn;

namespace JobsProviderApi.Endpoints.LinkedIn;

internal static class LinkedInJobsHandler
{
    public static async Task<IResult> HandleAsync(
        [AsParameters] JobSearchQuery query,
        ILinkedInJobsService jobsService,
        CancellationToken cancellationToken)
    {
        ListResponse<Job> result = await jobsService.SearchAsync(query, cancellationToken);
        return Results.Ok(result);
    }
}
