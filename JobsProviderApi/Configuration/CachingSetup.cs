using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace JobsProviderApi.Configuration;

public static class CachingSetup
{
    public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        var cachingOptions = configuration.GetSection(CachingOptions.SectionName).Get<CachingOptions>() ?? new CachingOptions();

        services.AddMemoryCache();
        services.Configure<CachingOptions>(configuration.GetSection(CachingOptions.SectionName));
        var fusionCacheBuilder = services.AddFusionCache()
            .WithDefaultEntryOptions(new FusionCacheEntryOptions
            {
                Duration = cachingOptions.SearchResultsDuration,
                DistributedCacheHardTimeout = TimeSpan.FromSeconds(5),
                AllowBackgroundDistributedCacheOperations = true,
            });

        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options => options.Configuration = redisConnectionString);
            fusionCacheBuilder
                .WithRegisteredDistributedCache()
                .WithSerializer(new FusionCacheSystemTextJsonSerializer());
        }

        return services;
    }
}
