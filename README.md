# JobsProviderApi

A minimal ASP.NET Core Web API that serves job postings over both REST and MCP. Data is served from a
static mock dataset (`JobsProviderApi/Data/mock-jobs.json`, 100 postings) split across two "sources":

- `GET /api/indeed/jobs` — first 50 postings
- `GET /api/stepstone/jobs` — remaining 50 postings

Both endpoints accept the same query parameters:

| Param                | Required | Description                                                                     |
|----------------------|----------|---------------------------------------------------------------------------------|
| `search`             | yes      | Case-insensitive plain-text match against a job's title or description.        |
| `countryCode`        | yes      | ISO 3166-1 alpha-2 code (e.g. `DE`) selecting which regional job board to search. |
| `mustHaveSkills`     | no       | Repeatable. ALL listed skills must appear in the job's requirements.            |
| `preferredSkills`    | no       | Repeatable. At least ONE listed skill must appear in the job's requirements.    |
| `preferredLocations` | no       | Repeatable. At least ONE listed location must be in the job's location.         |
| `skip`               | no       | Number of matching jobs to skip before applying `take`. Defaults to 0.         |
| `take`               | no       | Max number of jobs to return after `skip` is applied. Defaults to 10.          |

Skills match on exact equality (case-insensitive), so `Go` does not match `Golang`. Locations match on
substring, because a job's location is a composite string — `Berlin` matches `Berlin, Germany (Hybrid)`.
`search` is plain text, not a regex — special characters like `(` or `+` are matched literally.

`countryCode` is currently accepted but not yet applied to the results; the mock dataset has no per-market
split.

Both endpoints return `{ "totalCount": <jobs matching all filters, ignoring skip/take>, "items": [...] }`
instead of a bare array, so callers can page through results using `skip`/`take` and `totalCount`.

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
- `Models/` — `Job`, `JobSearchQuery`, `ListResponse<T>`.
- `Mcp/` — MCP tool definitions wrapping the REST services (`search_indeed_jobs`, `search_stepstone_jobs`).

## Caching

Search results and individual jobs (fetched from a search's returned page) are cached via
[FusionCache](https://github.com/ZiggyCreatures/FusionCache):

| Cache                    | Duration | Config key                  |
|---------------------------|----------|-----------------------------|
| Search results (`GET .../jobs`) | 3 hours  | `Caching:SearchResultsDuration` |
| Individual jobs (`GET .../jobs/{id}`) | 7 days   | `Caching:JobDuration`       |

`POST /api/cache/clear` flushes everything.

There's always an in-process (L1) cache. An optional Redis L2 is added on top **only if** a
`ConnectionStrings:Redis` value is configured — if it's absent, the app runs exactly as if Redis didn't exist.
Redis is treated as pure acceleration/persistence, never a hard dependency: a read that doesn't complete within
5 seconds (down, unreachable, slow, wrong credentials — anything) is treated as a cache miss and the request
falls through to the origin data instead of failing or hanging; writes to Redis happen in the background and
never add to request latency, so a request is never slowed down by however many jobs it's populating the cache
with.

The connection string is a secret and must never be committed. For local development:

```bash
dotnet user-secrets set "ConnectionStrings:Redis" "<host>:<port>,password=<password>,abortConnect=false" --project JobsProviderApi
```

In production (see `deploy/deploy.sh`), set the `REDIS_CONNECTION_STRING` env var before running the script and
it's passed into the container as `ConnectionStrings__Redis` (ASP.NET Core maps `__` to `:` in config keys);
leave it unset to run without Redis.

### Standing up Redis

`deploy/redis/deploy-redis.sh` starts a standalone Redis container (with AOF persistence to a named Docker
volume, so data survives a restart):

```bash
REDIS_PASSWORD="<a strong password>" deploy/redis/deploy-redis.sh
```

## Secrets

Secrets must never be committed. Locally they're stored outside the repo via [.NET user
secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets). The project already declares a
`UserSecretsId`, so there's nothing to initialize — just set values:

```bash
# Required: Apify API token, used to fetch real Indeed jobs.
dotnet user-secrets set "Apify:Token" "<your-apify-token>" --project JobsProviderApi

# Optional: Redis L2 cache connection string (omit to run without Redis).
dotnet user-secrets set "ConnectionStrings:Redis" "<host>:<port>,password=<password>,abortConnect=false" --project JobsProviderApi
```

Useful commands: `dotnet user-secrets list --project JobsProviderApi` to see what's set,
`dotnet user-secrets remove "Apify:Token" --project JobsProviderApi` to delete one, `dotnet user-secrets clear
--project JobsProviderApi` to wipe all.

The app fails fast at startup if `Apify:Token` is missing. In production, supply secrets as environment
variables instead of user secrets — ASP.NET Core maps `__` to `:`, so `Apify__Token` and
`ConnectionStrings__Redis` bind to the same keys (see `deploy/deploy.sh`).

## Running

```bash
dotnet run --project JobsProviderApi
```

Swagger UI is available at `/swagger` in development.

## Testing

```bash
dotnet test
```
