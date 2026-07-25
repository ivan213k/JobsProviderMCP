using JobsProviderApi.Endpoints.Cache;
using JobsProviderApi.Endpoints.Indeed;
using JobsProviderApi.Endpoints.Stepstone;
using JobsProviderApi.Providers;
using JobsProviderApi.Services;
using JobsProviderApi.Services.Indeed;
using JobsProviderApi.Services.Stepstone;
using ModelContextProtocol.Protocol;
using ZiggyCreatures.Caching.Fusion;

var builder = WebApplication.CreateBuilder(args);

var semanticVersion = Environment.GetEnvironmentVariable("SemanticVersion") ?? "dev";

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Version = semanticVersion;
        return Task.CompletedTask;
    });
});

builder.Services.AddMemoryCache();
builder.Services.AddFusionCache()
    .WithDefaultEntryOptions(new FusionCacheEntryOptions { Duration = TimeSpan.FromHours(3) });
builder.Services.AddValidation();
builder.Services.AddSingleton<IJobsProvider, MockJobsProvider>();
builder.Services.AddScoped<IJobSearchFilter, JobSearchFilter>();
builder.Services.AddScoped<IIndeedJobsService, IndeedJobsService>();
builder.Services.AddScoped<IStepstoneJobsService, StepstoneJobsService>();
builder.Services.AddMcpServer(options =>
{
    options.ServerInfo = new Implementation { Name = "JobsProviderApi", Version = semanticVersion };
}).WithHttpTransport(options => options.Stateless = true).WithToolsFromAssembly();

var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", $"JobsProviderApi {semanticVersion}");
});

app.UseHttpsRedirection();

app.MapIndeedJobsEndpoint();
app.MapStepstoneJobsEndpoint();
app.MapClearCacheEndpoint();
app.MapMcp("/mcp");

app.Run();
