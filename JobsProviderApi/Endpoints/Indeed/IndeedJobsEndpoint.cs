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

                Intended for use as an MCP tool by an agent searching for jobs matching a candidate profile. Only
                `search` is required; the rest are optional and combine with it using AND.

                - `search` (required): a regular expression matched case-insensitively against each job's title or
                  description.
                - `mustHaveSkills`: repeat the parameter for each skill (e.g. `?mustHaveSkills=Go&mustHaveSkills=gRPC`).
                  ALL listed skills must appear in the job's requirements list for the job to be returned.
                - `preferredSkills`: repeat the parameter for each skill. At least ONE listed skill must appear in
                  the job's requirements list for the job to be returned.
                - `take`: maximum number of jobs to return, applied after all other filters. Defaults to 10.
                """)
            .Produces<IReadOnlyList<Job>>()
            .ProducesValidationProblem();

        return app;
    }
}
