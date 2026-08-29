using ApifySdk;
using ApifySdk.Actors.Indeed;
using ApifySdk.Actors.LinkedIn;
using JobsProviderApi.Resilience;

namespace JobsProviderApi.Configuration;

public static class ApifySetup
{
    public static IServiceCollection AddApify(this IServiceCollection services, IConfiguration configuration)
    {
        ApifyOptions apifyOptions = configuration.GetSection(ApifyOptions.SectionName).Get<ApifyOptions>()
            ?? throw new InvalidOperationException($"Missing {ApifyOptions.SectionName} configuration section.");
        if (string.IsNullOrWhiteSpace(apifyOptions.Token))
        {
            throw new InvalidOperationException("Missing 'Apify:Token'. Set it via user-secrets or the Apify__Token environment variable.");
        }

        services.AddSingleton(apifyOptions);
        services
            .AddHttpClient<IApifyApiClient, ApifyApiClient>(client => client.Timeout = TimeSpan.FromSeconds(apifyOptions.TimeoutInSeconds))
            .AddApifyResilience();

        services.AddScoped<IIndeedActor, IndeedActor>();
        services.AddScoped<ILinkedInActor, LinkedInActor>();

        return services;
    }
}
