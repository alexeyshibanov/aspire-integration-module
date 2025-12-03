using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
using VirtoCommerce.Platform.Core.Logger;

namespace VirtoCommerce.AspireIntegrationModule.Web;

/// <summary>
/// Configures Serilog to send logs to OpenTelemetry when running under Aspire.
/// This integrates with the platform's Serilog configuration pipeline.
/// </summary>
public class OpenTelemetryLoggerConfigurationService : ILoggerConfigurationService
{
    private readonly IConfiguration _configuration;

    public OpenTelemetryLoggerConfigurationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(LoggerConfiguration loggerConfiguration)
    {
        // Only add OpenTelemetry sink if OTLP endpoint is configured (set by Aspire)
        var otlpEndpoint = _configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            loggerConfiguration.WriteTo.OpenTelemetry(options =>
            {
                options.Endpoint = otlpEndpoint;
                options.Protocol = OtlpProtocol.Grpc;

                // Include trace context for correlation with distributed traces
                options.IncludedData = IncludedData.TraceIdField | IncludedData.SpanIdField | IncludedData.MessageTemplateTextAttribute |
                                       IncludedData.MessageTemplateMD5HashAttribute;

                // Add resource attributes to identify the service
                var serviceName = _configuration["OTEL_SERVICE_NAME"] ?? "VirtoCommerce.Platform";
                options.ResourceAttributes = new Dictionary<string, object>
                {
                    ["service.name"] = serviceName,
                };
            });
        }
    }
}
