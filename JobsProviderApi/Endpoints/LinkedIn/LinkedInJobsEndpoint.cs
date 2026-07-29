using JobsProviderApi.Models;

namespace JobsProviderApi.Endpoints.LinkedIn;

public static class LinkedInJobsEndpoint
{
    public static IEndpointRouteBuilder MapLinkedInJobsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/linkedin/jobs", LinkedInJobsHandler.HandleAsync)
            .WithName("GetLinkedInJobs")
            .WithTags("LinkedIn")
            .WithSummary("Search LinkedIn job postings.")
            .WithDescription("""
                Returns LinkedIn job postings.

                Also available as the `search_linkedin_jobs` MCP tool (mounted at `/mcp`), which takes the same
                parameters and delegates to the same search logic.
                `search` and `countryCode` are required; the rest are optional and combined using AND.
                Results are sorted newest-first by date published.

                This source has no requirements field of its own, so `requirements` on each job lists the
                `mustHaveSkills`/`preferredSkills` from *this* request that were found in the posting, rather
                than everything the posting asks for. Skill matching is literal — `Kubernetes` will not match a
                posting that only says `K8s`.

                """)
            .Produces<ListResponse<Job>>()
            .ProducesValidationProblem();

        app.MapGet("/api/linkedin/jobs/{id}", LinkedInJobByIdHandler.HandleAsync)
            .WithName("GetLinkedInJobById")
            .WithTags("LinkedIn")
            .WithSummary("Get a single LinkedIn job posting by id.")
            .WithDescription("""
                Cache-only lookup: only returns a job that has previously been returned by a search against
                this source and whose individual cache entry hasn't expired. Returns 404 for a job id that
                exists in the dataset but was never searched for (or was matched but paginated past), and for
                one whose cache entry has expired — there is no fallback fetch.

                Also available as the `get_linkedin_job` MCP tool (mounted at `/mcp`).
                """)
            .Produces<Job>()
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
