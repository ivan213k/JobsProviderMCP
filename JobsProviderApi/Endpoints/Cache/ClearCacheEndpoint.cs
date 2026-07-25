namespace JobsProviderApi.Endpoints.Cache;

public static class ClearCacheEndpoint
{
    public static IEndpointRouteBuilder MapClearCacheEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/cache/clear", ClearCacheHandler.HandleAsync)
            .WithName("ClearCache")
            .WithTags("Cache")
            .WithSummary("Flush all cached search results.")
            .WithDescription("""
                Removes every cached Indeed/Stepstone search response, regardless of query, so the next search
                against each source is served fresh instead of from the cache. REST-only; not exposed as an MCP
                tool.
                """)
            .Produces(StatusCodes.Status204NoContent);

        return app;
    }
}
