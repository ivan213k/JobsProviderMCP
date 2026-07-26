using JobsProviderApi.Models;
using JobsProviderApi.Services.Indeed;

namespace JobsProviderApi.Endpoints.Indeed;

internal static class IndeedJobByIdHandler
{
    public static async Task<IResult> HandleAsync(string id, IIndeedJobsService jobsService, CancellationToken cancellationToken)
    {
        Job? job = await jobsService.GetByIdAsync(id, cancellationToken);
        return job is not null ? Results.Ok(job) : Results.NotFound();
    }
}
