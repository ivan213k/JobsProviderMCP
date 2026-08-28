# JobsProviderApi

A minimal ASP.NET Core Web API that serves job postings over both REST and MCP, from three sources:

| Source    | Endpoint                  | Backed by                                   |
|-----------|----------------------------|----------------------------------------------|
| Indeed    | `GET /api/indeed/jobs`     | live, via an Apify actor                     |
| LinkedIn  | `GET /api/linkedin/jobs`   | live, via an Apify actor                     |
| Stepstone | `GET /api/stepstone/jobs`  | static mock dataset                          |

Each source also has `GET .../jobs/{id}` for fetching a single previously-searched job by id (cache-only).

All endpoints accept the same query parameters: `search` (required text match), `countryCode` (required,
ISO 3166-1 alpha-2), plus optional `mustHaveSkills`, `preferredSkills`, `locations`, and `skip`/`take` for
pagination. Responses are shaped as `{ "totalCount": ..., "items": [...] }`.

LinkedIn has no public jobs API, so that source scrapes via an Apify actor — results take longer on a cache
miss (~2.5 min) and are cached for 3 hours; see comments in the code for the field-mapping details.

## MCP

The same functionality is exposed as an MCP server at `/mcp` (Streamable HTTP), with a search and a get-by-id
tool per source (e.g. `search_indeed_jobs` / `get_indeed_job`). Tools take the same parameters as the REST
endpoints and delegate to the same services, so results are identical either way.

## Caching

Search results (3h) and individual jobs (7d) are cached via [FusionCache](https://github.com/ZiggyCreatures/FusionCache).
`POST /api/cache/clear` flushes everything.

An optional Redis L2 cache is used if `ConnectionStrings:Redis` is configured; otherwise the app runs on an
in-process cache only. Redis is pure acceleration — any failure or slowness falls back to the origin data
rather than failing the request.

## Setup

```bash
# Required: Apify API token, used to fetch real Indeed and LinkedIn jobs.
dotnet user-secrets set "Apify:Token" "<your-apify-token>" --project JobsProviderApi

# Optional: Redis L2 cache connection string (omit to run without Redis).
dotnet user-secrets set "ConnectionStrings:Redis" "<host>:<port>,password=<password>,abortConnect=false" --project JobsProviderApi
```

To stand up a local Redis: `REDIS_PASSWORD="<a strong password>" deploy/redis/deploy-redis.sh`

## Running

```bash
dotnet run --project JobsProviderApi
```

Swagger UI is available at `/swagger` in development.

## Testing

```bash
dotnet test
```
