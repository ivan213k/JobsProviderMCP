using JobsProviderApi.Models;

namespace JobsProviderApi.Endpoints.Indeed;

public static class IndeedJobsEndpoint
{
    public static IEndpointRouteBuilder MapIndeedJobsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/indeed/jobs", IndeedJobsHandler.HandleAsync)
            .WithName("GetIndeedJobs")
            .WithTags("Indeed")
            .WithSummary("Search Indeed job postings.")
            .WithDescription("""
                Returns Indeed job postings.

                Also available as the `search_indeed_jobs` MCP tool (mounted at `/mcp`), which takes the same
                parameters and delegates to the same search logic.
                `search` and `countryCode` are required; the rest are optional and combined using AND.

                """)
            .Produces<IReadOnlyList<Job>>()
            .ProducesValidationProblem();

        return app;
    }
}
