using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace JobsProviderApi.Configuration;

public static class TelemetrySetup
{
    public static WebApplicationBuilder AddTelemetry(this WebApplicationBuilder builder, string serviceVersion)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("JobsProviderApi", serviceVersion: serviceVersion))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddFusionCacheInstrumentation()
                .AddSource("Polly")) // built-in Polly v8 resilience telemetry (see Resilience/ApifyResilience.cs)
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddFusionCacheInstrumentation()
                .AddMeter("Polly"))
            .UseOtlpExporter();

        return builder;
    }
}
