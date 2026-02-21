using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace VirtoCommerce.AspireIntegrationModule.Web;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAspireIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        // Note: OpenTelemetry logging is integrated via Serilog sink (see OpenTelemetryLoggerConfigurationService)
        // This avoids duplicate logging providers and follows VirtoCommerce Platform patterns

        // Configure OpenTelemetry metrics and tracing
        var otelBuilder = services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddEventCountersInstrumentation(options =>
                    {
                        options.AddEventSources("Microsoft.AspNetCore.Hosting", "Microsoft-AspNetCore-Server-Kestrel");
                    })
                    .AddMeter("Microsoft.EntityFrameworkCore", "Elastic.Transport");
            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddHangfireInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddElasticsearchClientInstrumentation(options =>
                    {
                        options.SuppressDownstreamInstrumentation = true;
                        options.ParseAndFormatRequest = true;
                    })
                    .AddSource("Elastic.Transport")
                    .AddRedisInstrumentation();
            });

        // Add OTLP exporter if configured
        var useOtlpExporter = !string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
        if (useOtlpExporter)
        {
            otelBuilder.UseOtlpExporter();
        }

        // Add health checks (adds to existing health checks)
        // Add a default liveness check to ensure app is responsive
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        // Add service discovery
        services.AddServiceDiscovery();

        // Configure HTTP client defaults
        services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        return services;
    }

    public static IApplicationBuilder UseAspireIntegration(this IApplicationBuilder appBuilder)
    {
        // Health checks are already registered in Initialize()
        // The platform's existing /health endpoint will include our health checks
        // Additional endpoints can be added here if needed, but for now we just return
        // since the platform already handles health check endpoint mapping

        return appBuilder;
    }
}
