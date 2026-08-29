using JobsProviderApi.Configuration;
using JobsProviderApi.Endpoints.Cache;
using JobsProviderApi.Endpoints.Indeed;
using JobsProviderApi.Endpoints.LinkedIn;
using JobsProviderApi.Endpoints.Stepstone;

var builder = WebApplication.CreateBuilder(args);

var semanticVersion = Environment.GetEnvironmentVariable("SemanticVersion") ?? "dev";

builder.Services.AddVersionedOpenApi(semanticVersion);
builder.Services.AddCaching(builder.Configuration);
builder.Services.AddValidation();
builder.Services.AddApify(builder.Configuration);
builder.Services.AddJobServices();
builder.Services.AddJobsMcp(semanticVersion);

var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", $"JobsProviderApi {semanticVersion}");
});

app.UseHttpsRedirection();

app.MapIndeedJobsEndpoint();
app.MapLinkedInJobsEndpoint();
app.MapStepstoneJobsEndpoint();
app.MapClearCacheEndpoint();
app.MapMcp("/mcp");

app.Run();
