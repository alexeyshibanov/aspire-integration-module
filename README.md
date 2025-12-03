# VirtoCommerce Aspire Integration Module

This module provides .NET Aspire integration for VirtoCommerce Platform, enabling OpenTelemetry observability, health checks, and service discovery when running under Aspire AppHost.

## Features

- **OpenTelemetry Integration**: Automatic metrics, tracing, and logging instrumentation
- **Health Checks**: Additional health check endpoints for Aspire monitoring
- **Service Discovery**: Automatic service-to-service discovery via Aspire
- **HTTP Resilience**: Built-in retry and circuit breaker patterns for HTTP clients
- **Conditional Activation**: Only enabled when running under Aspire or explicitly configured

## Prerequisites

- .NET 8.0
- VirtoCommerce Platform 3.916.0+
- .NET Aspire (for local development)

## Installation

1. Copy the module to your `modules` directory (or reference it from your module discovery path)
2. The module will be automatically discovered and loaded by VirtoCommerce Platform

## Configuration

### Automatic Detection

The module automatically detects when running under Aspire by checking for:
- `DOTNET_RESOURCE_SERVICE_ENDPOINT_URL` environment variable (set by Aspire)

### Manual Configuration

To explicitly enable/disable the module, add to `appsettings.json`:

```json
{
  "Aspire": {
    "Enabled": true  // Set to true to enable, false to disable
  }
}
```

### OpenTelemetry Configuration

The module automatically uses the OTLP exporter if `OTEL_EXPORTER_OTLP_ENDPOINT` is configured (which Aspire sets automatically).

## What Gets Registered

### In Initialize() (Service Registration)

- **OpenTelemetry Logging**: Integrated via Serilog sink using `ILoggerConfigurationService`
- **OpenTelemetry Metrics**: ASP.NET Core, HTTP Client, and Runtime instrumentation
- **OpenTelemetry Tracing**: ASP.NET Core and HTTP Client instrumentation
- **Health Checks**: Default liveness health check with tag `["live"]`
- **Service Discovery**: Automatic service endpoint resolution
- **HTTP Client Defaults**: Resilience patterns and service discovery

### In PostInitialize() (Middleware/Endpoints)

Currently, the module doesn't register additional endpoints as the platform already provides `/health` endpoint which includes all registered health checks.

## Usage with Aspire AppHost

When running under Aspire AppHost:

1. The module automatically detects Aspire environment variables
2. OpenTelemetry data flows to Aspire Dashboard
3. Health checks are available for monitoring
4. Service discovery enables automatic connection string injection

## Module Structure

```
aspire-integration-module/
└── src/
    └── VirtoCommerce.AspireIntegration.Web/
        ├── Module.cs                    # Module initialization
        ├── ServiceCollectionExtensions.cs  # Aspire service registration
        ├── module.manifest              # Module metadata
        └── VirtoCommerce.AspireIntegration.Web.csproj
```

## Dependencies

- **VirtoCommerce.Platform.Core** - Module system and ILoggerConfigurationService
- **OpenTelemetry packages** - Observability (metrics and tracing)
- **Serilog.Sinks.OpenTelemetry** - Serilog integration for OpenTelemetry logs
- **Microsoft.Extensions.ServiceDiscovery** - Service discovery
- **Microsoft.Extensions.Http.Resilience** - HTTP resilience patterns

## Notes

- This module is designed for **local development** with Aspire
- It does not modify core VirtoCommerce Platform code
- All Aspire features are conditionally enabled
- The module can be safely included in production builds but will only activate when Aspire environment variables are present

## Troubleshooting

### Module not activating

- Check that `Aspire:Enabled` is set to `true` in appsettings.json, OR
- Verify that Aspire environment variables are present when running under AppHost

### OpenTelemetry not working

- Ensure `OTEL_EXPORTER_OTLP_ENDPOINT` is configured (Aspire sets this automatically)
- Check Aspire Dashboard for telemetry data

### Health checks not appearing

- Health checks are included in the platform's existing `/health` endpoint
- The module adds health checks but doesn't create new endpoints

