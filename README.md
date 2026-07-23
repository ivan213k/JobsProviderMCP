# JobsProviderApi

A minimal ASP.NET Core Web API that serves job postings over both REST and MCP. Data is served from a
static mock dataset (`JobsProviderApi/Data/mock-jobs.json`, 100 postings) split across two "sources":

- `GET /api/indeed/jobs` — first 50 postings
- `GET /api/stepstone/jobs` — remaining 50 postings

Both endpoints accept the same query parameters:

| Param                | Required | Description                                                                     |
|----------------------|----------|---------------------------------------------------------------------------------|
| `search`             | yes      | Regex matched case-insensitively against a job's title or description.          |
| `countryCode`        | yes      | ISO 3166-1 alpha-2 code (e.g. `DE`) selecting which regional job board to search. |
| `mustHaveSkills`     | no       | Repeatable. ALL listed skills must appear in the job's requirements.            |
| `preferredSkills`    | no       | Repeatable. At least ONE listed skill must appear in the job's requirements.    |
| `preferredLocations` | no       | Repeatable. At least ONE listed location must be in the job's location.         |
| `take`               | no       | Max number of jobs to return, applied after filtering. Defaults to 10.          |

Skills match on exact equality (case-insensitive), so `Go` does not match `Golang`. Locations match on
substring, because a job's location is a composite string — `Berlin` matches `Berlin, Germany (Hybrid)`.

`countryCode` is currently accepted and validated but not yet applied to the results; the mock dataset has no
per-market split.

An invalid `search` regex is rejected with a `400` before any handler code runs, via ASP.NET Core's built-in
minimal API validation (`JobSearchQuery` implements `IValidatableObject`).

## MCP

The same search functionality is also exposed as an MCP server, mounted at `/mcp` (Streamable HTTP), with one
tool per source:

- `search_indeed_jobs`
- `search_stepstone_jobs`

Both tools take the same parameters as the REST endpoints above and delegate to the same underlying services,
so results are identical either way.

## Project layout

- `Endpoints/<Source>/` — route mapping + request handler per job source.
- `Services/<Source>/` — per-source service (dataset slice + delegates filtering).
- `Services/JobSearchFilter.cs` — shared filtering logic (search/skills/locations/take), used by both sources.
- `Providers/MockJobsProvider.cs` — reads `Data/mock-jobs.json`.
- `Models/` — `Job`, `JobSearchQuery`.
- `Mcp/` — MCP tool definitions wrapping the REST services (`search_indeed_jobs`, `search_stepstone_jobs`).

## Running

```bash
dotnet run --project JobsProviderApi
```

Swagger UI is available at `/swagger` in development.

## Testing

```bash
dotnet test
```
