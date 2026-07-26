using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace JobsProviderApi.Resilience;

public static class ApifyResilience
{
    public static IHttpClientBuilder AddApifyResilience(this IHttpClientBuilder builder)
    {
        builder.AddResilienceHandler("apify", pipeline =>
        {
            pipeline.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true
            });
        });

        return builder;
    }
}
