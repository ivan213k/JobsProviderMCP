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

Each source also has a `GET .../jobs/{id}` for fetching a single previously-searched job by id (cache-only, see
Caching below) and a matching MCP tool (`get_indeed_job`/`get_stepstone_job`).

Both share one query contract, `Models/JobSearchQuery.cs`: `search` (required, case-insensitive plain-text match
against title or description — not a regex), `mustHaveSkills`/`preferredSkills`/`locations` (optional,
repeatable), `skip`/`take` (optional, default 0/10) for pagination. Both entry points return
`Models/ListResponse.cs`'s `ListResponse<Job>` — `totalCount` (jobs matching all filters, before `skip`/`take`)
plus `items` (the requested page) — rather than a bare array, so callers can page through results.

**Request flow**, per source (e.g. Indeed):
`Endpoints/Indeed/IndeedJobsEndpoint.cs` (route + OpenAPI metadata) → `Endpoints/Indeed/IndeedJobsHandler.cs`
(binds `JobSearchQuery` via `[AsParameters]`, calls the service) → `Services/Indeed/IndeedJobsService.cs`
(fetches all jobs from `IJobsProvider`, takes/skips the 50-item slice, delegates filtering) →
`Services/JobSearchFilter.cs` (shared plain-text search + must-have/preferred-skill/location filtering, sorted
newest-first by `DatePublished`, then `skip`/`take` pagination, used by both sources).

The Endpoints/ and Services/ trees both mirror the two-source split (`Indeed/`, `Stepstone/`), while anything
source-agnostic (`JobSearchFilter`, `IJobsProvider`/`MockJobsProvider`) stays at the top level of its folder.

`JobSearchQuery` has no bespoke validation — plain-text search can't be malformed the way a regex could, so
there's nothing for `JobSearchFilter` (or the MCP tools) to reject; `builder.Services.AddValidation()` in
`Program.cs` still covers ASP.NET Core's automatic minimal-API required-parameter checks (e.g. missing
`search`/`countryCode`) for the REST endpoints.

`MockJobsProvider` itself re-reads and re-parses `Data/mock-jobs.json` on every call — deliberate, since
`IndeedJobsService`/`StepstoneJobsService` cache above it (see Caching below), so there was no reason to also
cache inside the provider.

All per-source services are registered in DI behind interfaces (`IIndeedJobsService`, `IStepstoneJobsService`,
`IJobSearchFilter`, `IJobsProvider`), which is what makes them mockable/fakeable in `JobsProviderApi.Tests`
(see `Tests/Fakes/FakeJobsProvider.cs` and `Tests/TestJobs.cs` for the test data builder).

**MCP server**: `Mcp/IndeedJobSearchTool.cs` and `Mcp/StepstoneJobSearchTool.cs` expose `search_indeed_jobs` and
`search_stepstone_jobs`; `Mcp/IndeedJobByIdTool.cs` and `Mcp/StepstoneJobByIdTool.cs` expose `get_indeed_job` and
`get_stepstone_job`. Every tool is a thin wrapper that delegates to the same `IIndeedJobsService`/
`IStepstoneJobsService` the REST endpoints use (`SearchAsync`/`GetByIdAsync`) — no filtering or caching logic is
duplicated between REST and MCP. Per-field `[Description]` text for search is shared between the REST model and
both search tools via `Models/JobSearchQuery.cs`'s `JobSearchQueryDescriptions` constants, so the two entry
points can't drift out of sync on wording. `Program.cs` registers the server with
`builder.Services.AddMcpServer(...).WithHttpTransport(...).WithToolsFromAssembly()` (setting `ServerInfo` to the
same `SemanticVersion`-derived version used for the OpenAPI doc) and mounts it at `/mcp` via `app.MapMcp("/mcp")`,
using Streamable HTTP in stateless mode (no per-session state — the actual cross-request state lives in
FusionCache, not in the MCP transport). MCP tool parameters that are optional in `JobSearchQuery`
(`mustHaveSkills`, `preferredSkills`, `locations`, `skip`, `take`) must have C# default values (`= null`/`= 0`/
`= 10`), not just nullable types — the MCP SDK derives its "required" schema from whether a parameter has a
default, not from nullability, so omitting the default makes a client-omitted argument fail at the SDK layer
before the tool method ever runs.

## Caching

Both search (`SearchAsync`) and get-by-id (`GetByIdAsync`) are cached in `IndeedJobsService`/`StepstoneJobsService`
— the service layer is the single choke point both REST and MCP funnel through, so caching there (like
filtering in `JobSearchFilter`) automatically covers both entry points without duplicating logic. There is no
per-endpoint or per-tool caching.

- **Search results**: `cache.GetOrSetAsync` keyed by `JobSearchQuery.ToCacheKey(source)` — the whole query
  record JSON-serialized and prefixed `"indeed"`/`"stepstone"`, so the key can't drift out of sync if
  `JobSearchQuery` gains a field, and the two sources can't collide on the same query text. Duration:
  `CachingOptions.SearchResultsDuration` (3h default), applied via `WithDefaultEntryOptions` in `Program.cs`.
- **Individual jobs**: as a side effect of a search-cache *miss* (inside `SearchUncachedAsync`), every job in
  the returned page is separately cached via `Job.ToCacheKey(source, id)` (`"indeed:job:42"` etc.), for
  `CachingOptions.JobDuration` (7d default). `GetByIdAsync` is a pure `TryGetAsync` read of that key — no
  fallback fetch. This means a job is only reachable by id once some search has returned it on a page (post
  `skip`/`take`); one that exists in the dataset but was never searched for, or was matched but paginated past,
  404s/returns `null`. Deliberate, confirmed tradeoff — not an oversight.
- **`CachingOptions`** (`Configuration/CachingOptions.cs`) binds the `Caching` section of `appsettings.json` —
  durations are config, not hardcoded, and both endpoints/tools document them in their OpenAPI/MCP descriptions.
- **`POST /api/cache/clear`** (`Endpoints/Cache/`) calls `IFusionCache.ClearAsync(allowFailSafe: false)` — a
  true flush (removes entries), not FusionCache's default expire-but-retain-for-fail-safe behavior. REST-only,
  no MCP tool, by design.
- **Redis (L2), optional**: wired in `Program.cs` only if `ConnectionStrings:Redis` is configured (`dotnet
  user-secrets` locally, `ConnectionStrings__Redis` env var in prod — see README). When present,
  `DistributedCacheHardTimeout = 5s` bounds L2 *reads* so a slow/unreachable Redis can't hang a request past 5s
  (FusionCache already swallows L2 exceptions by default via `ReThrowDistributedCacheExceptions = false`).
  `AllowBackgroundDistributedCacheOperations = true` is what keeps L2 *writes* off the request's critical path —
  added after finding that without it, `CacheJobsByIdAsync`'s per-job `SetAsync` loop stacked a 5s wait *per job
  on the page* when Redis was down (29s observed for a 3-job page), since each write blocked in turn before the
  next started.
  **Gotcha**: job-level entry options must be built via `cache.DefaultEntryOptions.Duplicate(jobDuration)`, not
  `new FusionCacheEntryOptions { Duration = jobDuration }` — a fresh instance silently drops the hard-timeout/
  background-ops settings from the cache-wide defaults, re-exposing the stacking-timeout bug for that path.
