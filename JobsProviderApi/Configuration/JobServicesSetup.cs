using JobsProviderApi.Providers;
using JobsProviderApi.Services;
using JobsProviderApi.Services.Indeed;
using JobsProviderApi.Services.LinkedIn;
using JobsProviderApi.Services.Stepstone;

namespace JobsProviderApi.Configuration;

public static class JobServicesSetup
{
    public static IServiceCollection AddJobServices(this IServiceCollection services)
    {
        services.AddScoped<IIndeedJobsProvider, IndeedJobsProvider>();
        services.AddScoped<ILinkedInJobsProvider, LinkedInJobsProvider>();
        services.AddSingleton<IStepstoneJobsProvider, MockJobsProvider>();
        services.AddScoped<IJobSearchFilter, JobSearchFilter>();
        services.AddScoped<IIndeedJobsService, IndeedJobsService>();
        services.AddScoped<ILinkedInJobsService, LinkedInJobsService>();
        services.AddScoped<IStepstoneJobsService, StepstoneJobsService>();

        return services;
    }
}
