using JobsProviderApi.Endpoints.Indeed;
using JobsProviderApi.Endpoints.Stepstone;
using JobsProviderApi.Providers;
using JobsProviderApi.Services;
using JobsProviderApi.Services.Indeed;
using JobsProviderApi.Services.Stepstone;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddValidation();
builder.Services.AddSingleton<IJobsProvider, MockJobsProvider>();
builder.Services.AddScoped<IJobSearchFilter, JobSearchFilter>();
builder.Services.AddScoped<IIndeedJobsService, IndeedJobsService>();
builder.Services.AddScoped<IStepstoneJobsService, StepstoneJobsService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "JobsProviderApi v1");
    });
}

app.UseHttpsRedirection();

app.MapIndeedJobsEndpoint();
app.MapStepstoneJobsEndpoint();

app.Run();
