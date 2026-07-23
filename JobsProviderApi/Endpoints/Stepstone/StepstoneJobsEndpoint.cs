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

                """)
            .Produces<IReadOnlyList<Job>>()
            .ProducesValidationProblem();

        return app;
    }
}
