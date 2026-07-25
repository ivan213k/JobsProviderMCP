using JobsProviderApi.Models;

namespace JobsProviderApi.Endpoints.Stepstone;

public static class StepstoneJobsEndpoint
{
    public static IEndpointRouteBuilder MapStepstoneJobsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/stepstone/jobs", StepstoneJobsHandler.HandleAsync)
            .WithName("GetStepstoneJobs")
            .WithTags("Stepstone")
            .WithSummary("Search Stepstone job postings.")
            .WithDescription("""
                Returns Stepstone job postings.

                Also available as the `search_stepstone_jobs` MCP tool (mounted at `/mcp`), which takes the same
                parameters and delegates to the same search logic.
                `search` and `countryCode` are required; the rest are optional and combined using AND.
                Results are sorted newest-first by date published.

                """)
            .Produces<ListResponse<Job>>()
            .ProducesValidationProblem();

        app.MapGet("/api/stepstone/jobs/{id}", StepstoneJobByIdHandler.HandleAsync)
            .WithName("GetStepstoneJobById")
            .WithTags("Stepstone")
            .WithSummary("Get a single Stepstone job posting by id.")
            .WithDescription("""
                Cache-only lookup: only returns a job that has previously been returned by a search against
                this source and whose individual cache entry hasn't expired. Returns 404 for a job id that
                exists in the dataset but was never searched for (or was matched but paginated past), and for
                one whose cache entry has expired — there is no fallback fetch.

                Also available as the `get_stepstone_job` MCP tool (mounted at `/mcp`).
                """)
            .Produces<Job>()
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
