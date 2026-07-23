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

Both share one query contract, `Models/JobSearchQuery.cs`: `search` (required, case-insensitive plain-text match
against title or description — not a regex), `mustHaveSkills`/`preferredSkills`/`locations` (optional,
repeatable), `skip`/`take` (optional, default 0/10) for pagination. Both entry points return
`Models/ListResponse.cs`'s `ListResponse<Job>` — `totalCount` (jobs matching all filters, before `skip`/`take`)
plus `items` (the requested page) — rather than a bare array, so callers can page through results.

**Request flow**, per source (e.g. Indeed):
`Endpoints/Indeed/IndeedJobsEndpoint.cs` (route + OpenAPI metadata) → `Endpoints/Indeed/IndeedJobsHandler.cs`
(binds `JobSearchQuery` via `[AsParameters]`, calls the service) → `Services/Indeed/IndeedJobsService.cs`
(fetches all jobs from `IJobsProvider`, takes/skips the 50-item slice, delegates filtering) →
`Services/JobSearchFilter.cs` (shared plain-text search + must-have/preferred-skill/location filtering, then
`skip`/`take` pagination, used by both sources).

The Endpoints/ and Services/ trees both mirror the two-source split (`Indeed/`, `Stepstone/`), while anything
source-agnostic (`JobSearchFilter`, `IJobsProvider`/`MockJobsProvider`) stays at the top level of its folder.

`JobSearchQuery` has no bespoke validation — plain-text search can't be malformed the way a regex could, so
there's nothing for `JobSearchFilter` (or the MCP tools) to reject; `builder.Services.AddValidation()` in
`Program.cs` still covers ASP.NET Core's automatic minimal-API required-parameter checks (e.g. missing
`search`/`countryCode`) for the REST endpoints.

`MockJobsProvider` re-reads and re-parses `Data/mock-jobs.json` on every call (no caching) — this was a
deliberate simplification, not an oversight.

All per-source services are registered in DI behind interfaces (`IIndeedJobsService`, `IStepstoneJobsService`,
`IJobSearchFilter`, `IJobsProvider`), which is what makes them mockable/fakeable in `JobsProviderApi.Tests`
(see `Tests/Fakes/FakeJobsProvider.cs` and `Tests/TestJobs.cs` for the test data builder).

**MCP server**: `Mcp/IndeedJobSearchTool.cs` and `Mcp/StepstoneJobSearchTool.cs` expose `search_indeed_jobs` and
`search_stepstone_jobs` as MCP tools, each a thin wrapper that builds a `JobSearchQuery` from flat parameters
and delegates to the same `IIndeedJobsService`/`IStepstoneJobsService` the REST endpoints use — no filtering
logic is duplicated. Per-field `[Description]` text is shared between the REST model and both MCP tools via
`Models/JobSearchQuery.cs`'s `JobSearchQueryDescriptions` constants, so the two entry points can't drift out of
sync on wording. `Program.cs` registers the server with
`builder.Services.AddMcpServer(...).WithHttpTransport(...).WithToolsFromAssembly()` (setting `ServerInfo` to the
same `SemanticVersion`-derived version used for the OpenAPI doc) and mounts it at `/mcp` via `app.MapMcp("/mcp")`,
using Streamable HTTP in stateless mode (no per-session state, consistent with `MockJobsProvider`'s no-caching
design). MCP tool parameters that are optional in `JobSearchQuery` (`mustHaveSkills`, `preferredSkills`,
`locations`, `skip`, `take`) must have C# default values (`= null`/`= 0`/`= 10`), not just nullable types — the
MCP SDK derives its "required" schema from whether a parameter has a default, not from nullability, so omitting
the default makes a client-omitted argument fail at the SDK layer before the tool method ever runs.
