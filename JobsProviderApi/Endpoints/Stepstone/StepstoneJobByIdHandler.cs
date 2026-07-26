using JobsProviderApi.Models;
using JobsProviderApi.Services.Stepstone;

namespace JobsProviderApi.Endpoints.Stepstone;

internal static class StepstoneJobByIdHandler
{
    public static async Task<IResult> HandleAsync(string id, IStepstoneJobsService jobsService, CancellationToken cancellationToken)
    {
        Job? job = await jobsService.GetByIdAsync(id, cancellationToken);
        return job is not null ? Results.Ok(job) : Results.NotFound();
    }
}
