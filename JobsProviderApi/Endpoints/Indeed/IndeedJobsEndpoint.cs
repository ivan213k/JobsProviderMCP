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

                Intended for use as an MCP tool by an agent searching for jobs matching a candidate profile.
                `search` and `countryCode` are required; the rest are optional and combined using AND.

                - `search` (required): plain text matched case-insensitively as a substring of each job's title or
                  description.
                - `mustHaveSkills`: repeat the parameter for each skill (e.g. `?mustHaveSkills=Go&mustHaveSkills=gRPC`).
                  ALL listed skills must appear in the job's requirements list for the job to be returned.
                - `preferredSkills`: repeat the parameter for each skill. At least ONE listed skill must appear in
                  the job's requirements list for the job to be returned.
                - `locations`: repeat the parameter for each location (e.g.
                  `?locations=Berlin&locations=Remote`). At least ONE listed location must appear
                  in the job's location for the job to be returned. Matching is a case-insensitive substring, so
                  `Berlin` matches `Berlin, Germany (Hybrid)`.
                - `countryCode` (required): ISO 3166-1 alpha-2 code (e.g. `DE`) identifying which regional job
                  board to search. It selects the market rather than filtering the results.
                - `take`: maximum number of jobs to return, applied after all other filters. Defaults to 100.
                """)
            .Produces<IReadOnlyList<Job>>()
            .ProducesValidationProblem();

        return app;
    }
}
