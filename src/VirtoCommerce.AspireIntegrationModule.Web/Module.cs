using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.Platform.Core.Logger;
using VirtoCommerce.Platform.Core.Modularity;

namespace VirtoCommerce.AspireIntegrationModule.Web;

public class Module : IModule, IHasConfiguration
{
    public required ManifestModuleInfo ModuleInfo { get; set; }

    public required IConfiguration Configuration { get; set; }

    public void Initialize(IServiceCollection serviceCollection)
    {
        var isAspire = Configuration.GetValue("Aspire:Enabled", false) || Environment.GetEnvironmentVariable("DOTNET_RESOURCE_SERVICE_ENDPOINT_URL") != null;
        if (isAspire)
        {
            // Register Serilog OpenTelemetry sink configuration
            // This integrates with the platform's Serilog pipeline instead of creating a duplicate logging provider
            serviceCollection.AddTransient<ILoggerConfigurationService, OpenTelemetryLoggerConfigurationService>();

            // Register Aspire integration (metrics, tracing, health checks, service discovery)
            serviceCollection.AddAspireIntegration(Configuration);
        }
    }

    public void PostInitialize(IApplicationBuilder appBuilder)
    {
        var isAspire = Configuration.GetValue("Aspire:Enabled", false) || Environment.GetEnvironmentVariable("DOTNET_RESOURCE_SERVICE_ENDPOINT_URL") != null;
        if (isAspire)
        {
            appBuilder.UseAspireIntegration();
        }
    }

    public void Uninstall()
    {
        // Nothing to do
    }
}
