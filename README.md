# JobsProviderApi

A minimal ASP.NET Core Web API that serves job postings over both REST and MCP, from three sources:

| Source    | Endpoint                 | Backed by                                            |
|-----------|--------------------------|------------------------------------------------------|
| Indeed    | `GET /api/indeed/jobs`   | live, via the `valig/indeed-jobs-scraper` Apify actor |
| LinkedIn  | `GET /api/linkedin/jobs` | live, via the `cheap_scraper/linkedin-job-scraper` Apify actor |
| Stepstone | `GET /api/stepstone/jobs`| static mock dataset (`JobsProviderApi/Data/mock-jobs.json`) |

Each source also has `GET .../jobs/{id}` for fetching a single previously-searched job by id (cache-only — see
[Caching](#caching)).

All endpoints accept the same query parameters:

| Param                | Required | Description                                                                     |
|----------------------|----------|---------------------------------------------------------------------------------|
| `search`             | yes      | Case-insensitive plain-text match against a job's title or description.        |
| `countryCode`        | yes      | ISO 3166-1 alpha-2 code (e.g. `DE`) selecting which regional job board to search. |
| `mustHaveSkills`     | no       | Repeatable. ALL listed skills must appear in the job's requirements.            |
| `preferredSkills`    | no       | Repeatable. At least ONE listed skill must appear in the job's requirements.    |
| `locations`          | no       | Repeatable. At least ONE listed location must be in the job's location.         |
| `skip`               | no       | Number of matching jobs to skip before applying `take`. Defaults to 0.         |
| `take`               | no       | Max number of jobs to return after `skip` is applied. Defaults to 10.          |

Skills match on exact equality (case-insensitive), so `Go` does not match `Golang`. Locations match on
substring, because a job's location is a composite string — `Berlin` matches `Berlin, Germany (Hybrid)`.
`search` is plain text, not a regex — special characters like `(` or `+` are matched literally.

`countryCode` is validated as a 2-letter ISO 3166-1 alpha-2 code; anything else is rejected with `400`. Indeed
passes it to the actor as the regional job board. LinkedIn's actor has no country input, so it is used as the
search location only when no `locations` are given (see below). Stepstone is still mock data with no per-market
split, so it accepts the code without applying it.

All endpoints return `{ "totalCount": <jobs matching all filters, ignoring skip/take>, "items": [...] }`
instead of a bare array, so callers can page through results using `skip`/`take` and `totalCount`.

## LinkedIn specifics

LinkedIn has no public jobs API, so this source scrapes via an Apify actor. Two consequences worth knowing
before using it:

**`requirements` means something different here.** The actor exposes no requirements field, but it does keyword
matching: `mustHaveSkills` and `preferredSkills` are sent to it as `resumeKeywords`, and the subset it finds in
each posting comes back as `matchedKeywords`, which becomes the job's `requirements`. So `requirements` lists
the skills *you asked about* that the posting mentions — not everything the posting asks for. Matching is
literal, so `Kubernetes` will not match a posting that only says `K8s` (the actor supports aliases; they are not
wired up yet).

A side effect: because `requirements` depends on the query but the per-job cache key does not, `GET
/api/linkedin/jobs/{id}` returns the value from whichever search last returned that job — possibly someone
else's. Search results themselves are unaffected, being cached per query.

**Runs are slow and metered.** The actor is billed per result with a 150-result minimum, and a run takes ~2.5
minutes, so an uncached LinkedIn search blocks for that long — hence `Apify:TimeoutInSeconds` at 280 (Apify's
own synchronous-run limit is 300s). Results are cached for 3 hours, so repeat queries are free and instant.
Postings are limited to the last 30 days; the actor offers only 24h/7d/30d windows.

## MCP

The same functionality is also exposed as an MCP server, mounted at `/mcp` (Streamable HTTP), with a search and
a get-by-id tool per source:

- `search_indeed_jobs` / `get_indeed_job`
- `search_linkedin_jobs` / `get_linkedin_job`
- `search_stepstone_jobs` / `get_stepstone_job`

The tools take the same parameters as the REST endpoints above and delegate to the same underlying services, so
results are identical either way.

## Project layout

- `Endpoints/<Source>/` — route mapping + request handler per job source.
- `Services/<Source>/` — per-source service (caching + delegates filtering).
- `Services/JobSearchFilter.cs` — shared filtering logic (skills/locations/sort/paging), used by all sources.
- `Providers/` — per-source `IJobsProvider` implementations: `IndeedJobsProvider` and `LinkedInJobsProvider`
  map a `JobSearchQuery` onto their Apify actor and the actor's results back onto `Job`; `MockJobsProvider`
  reads `Data/mock-jobs.json`.
- `Models/` — `Job`, `JobSearchQuery`, `ListResponse<T>`.
- `Mcp/` — MCP tool definitions wrapping the same services the REST endpoints use.
- `../ApifySdk/Actors/<Source>/` — the Apify actor client and its request/result models, one folder per actor.

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
# Required: Apify API token, used to fetch real Indeed and LinkedIn jobs.
dotnet user-secrets set "Apify:Token" "<your-apify-token>" --project JobsProviderApi

# Optional: Redis L2 cache connection string (omit to run without Redis).
dotnet user-secrets set "ConnectionStrings:Redis" "<host>:<port>,password=<password>,abortConnect=false" --project JobsProviderApi
```

## Running

```bash
dotnet run --project JobsProviderApi
```

Swagger UI is available at `/swagger` in development.

## Testing

```bash
dotnet test
```
