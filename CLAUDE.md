# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
dotnet build                                              # build everything (solution: JobsProviderApi.slnx)
dotnet test                                                # run all tests
dotnet test --filter "FullyQualifiedName~JobSearchFilterTests"   # run one test class
dotnet test --filter "Apply_WithTake_LimitsResults"        # run one test by name
dotnet run --project JobsProviderApi                       # run the API (Swagger UI at /swagger in Development)
```

No lint/format command is configured.

## Architecture

ASP.NET Core minimal API (net10.0) that serves job postings from a static mock dataset
(`JobsProviderApi/Data/mock-jobs.json`, 100 entries), exposed both as REST endpoints and as an in-process MCP
server. There are two "sources," which are really just two slices of the same dataset:

- `GET /api/indeed/jobs` — first 50 entries
- `GET /api/stepstone/jobs` — remaining 50 entries

Both share one query contract, `Models/JobSearchQuery.cs`: `search` (required regex, matched against title or
description), `mustHaveSkills`/`preferredSkills` (optional, repeatable), `take` (optional, default 10).

**Request flow**, per source (e.g. Indeed):
`Endpoints/Indeed/IndeedJobsEndpoint.cs` (route + OpenAPI metadata) → `Endpoints/Indeed/IndeedJobsHandler.cs`
(binds `JobSearchQuery` via `[AsParameters]`, calls the service) → `Services/Indeed/IndeedJobsService.cs`
(fetches all jobs from `IJobsProvider`, takes/skips the 50-item slice, delegates filtering) →
`Services/JobSearchFilter.cs` (shared regex + must-have/preferred-skill + take logic, used by both sources).

The Endpoints/ and Services/ trees both mirror the two-source split (`Indeed/`, `Stepstone/`), while anything
source-agnostic (`JobSearchFilter`, `IJobsProvider`/`MockJobsProvider`) stays at the top level of its folder.

**Validation is API-level, not filter-level**: `JobSearchQuery` implements `IValidatableObject` to check that
`search` is a syntactically valid regex. `Program.cs` calls `builder.Services.AddValidation()`, which is
ASP.NET Core's built-in automatic minimal-API validation — it rejects bad input with a `400
ValidationProblemDetails` before the handler or `JobSearchFilter` ever run. `JobSearchFilter` therefore assumes
its input is already valid and does no defensive checking of its own.

`MockJobsProvider` re-reads and re-parses `Data/mock-jobs.json` on every call (no caching) — this was a
deliberate simplification, not an oversight.

All per-source services are registered in DI behind interfaces (`IIndeedJobsService`, `IStepstoneJobsService`,
`IJobSearchFilter`, `IJobsProvider`), which is what makes them mockable/fakeable in `JobsProviderApi.Tests`
(see `Tests/Fakes/FakeJobsProvider.cs` and `Tests/TestJobs.cs` for the test data builder).

**MCP server**: `Mcp/IndeedJobSearchTool.cs` and `Mcp/StepstoneJobSearchTool.cs` expose `search_indeed_jobs` and
`search_stepstone_jobs` as MCP tools, each a thin wrapper that builds a `JobSearchQuery` from flat parameters
and delegates to the same `IIndeedJobsService`/`IStepstoneJobsService` the REST endpoints use — no filtering
logic is duplicated. `Program.cs` registers the server with
`builder.Services.AddMcpServer().WithHttpTransport(...).WithToolsFromAssembly()` and mounts it at `/mcp` via
`app.MapMcp("/mcp")`, using Streamable HTTP in stateless mode (no per-session state, consistent with
`MockJobsProvider`'s no-caching design). Because `AddValidation()` only wires into the HTTP minimal-API
pipeline, MCP tool methods validate the constructed `JobSearchQuery` themselves via
`Validator.TryValidateObject` (see `Mcp/McpValidation.cs`) and throw `McpException` on failure, replicating the
same regex-validation behavior the REST endpoints get for free.
