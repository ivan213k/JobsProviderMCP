using ApifySdk;
using ApifySdk.Actors.Indeed;
using JobsProviderApi.Endpoints.Indeed;
using JobsProviderApi.Endpoints.Stepstone;
using JobsProviderApi.Providers;
using JobsProviderApi.Providers.Indeed;
using JobsProviderApi.Resilience;
using JobsProviderApi.Services;
using JobsProviderApi.Services.Indeed;
using JobsProviderApi.Services.Stepstone;
using ModelContextProtocol.Protocol;

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
builder.Services.AddValidation();

ApifyOptions apifyOptions = builder.Configuration.GetSection(ApifyOptions.SectionName).Get<ApifyOptions>()
    ?? throw new InvalidOperationException($"Missing {ApifyOptions.SectionName} configuration section.");
if (string.IsNullOrWhiteSpace(apifyOptions.Token))
{
    throw new InvalidOperationException("Missing 'Apify:Token'. Set it via user-secrets or the Apify__Token environment variable.");
}

builder.Services.AddSingleton(apifyOptions);
builder.Services
    .AddHttpClient<IApifyApiClient, ApifyApiClient>(client => client.Timeout = TimeSpan.FromSeconds(apifyOptions.TimeoutInSeconds))
    .AddApifyResilience();

builder.Services.AddScoped<IIndeedActor, IndeedActor>();

// One provider per source. Swap any source onto the mock dataset by pointing its registration at
// MockJobsProvider<TSource>.
builder.Services.AddScoped<IJobsProvider<IndeedSource>, IndeedJobsProvider>();
builder.Services.AddSingleton<IJobsProvider<StepstoneSource>, MockJobsProvider<StepstoneSource>>();
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
app.MapMcp("/mcp");

app.Run();
