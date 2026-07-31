using JobsProviderApi.Models;
using JobsProviderApi.Services.LinkedIn;

namespace JobsProviderApi.Endpoints.LinkedIn;

internal static class LinkedInJobByIdHandler
{
    public static async Task<IResult> HandleAsync(string id, ILinkedInJobsService jobsService, CancellationToken cancellationToken)
    {
        Job? job = await jobsService.GetByIdAsync(id, cancellationToken);
        return job is not null ? Results.Ok(job) : Results.NotFound();
    }
}
