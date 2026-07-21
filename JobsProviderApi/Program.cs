using JobsProviderApi.Endpoints.Indeed;
using JobsProviderApi.Endpoints.Stepstone;
using JobsProviderApi.Providers;
using JobsProviderApi.Services;
using JobsProviderApi.Services.Indeed;
using JobsProviderApi.Services.Stepstone;

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
builder.Services.AddSingleton<IJobsProvider, MockJobsProvider>();
builder.Services.AddScoped<IJobSearchFilter, JobSearchFilter>();
builder.Services.AddScoped<IIndeedJobsService, IndeedJobsService>();
builder.Services.AddScoped<IStepstoneJobsService, StepstoneJobsService>();

var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", $"JobsProviderApi {semanticVersion}");
});

app.UseHttpsRedirection();

app.MapIndeedJobsEndpoint();
app.MapStepstoneJobsEndpoint();

app.Run();
