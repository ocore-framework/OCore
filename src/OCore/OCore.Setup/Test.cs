﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OCore.Authorization;
using OCore.Diagnostics;
using Orleans.Hosting;
using Orleans.Providers;
using System;
using System.Threading.Tasks;
using OCore.Services;
using Orleans;

namespace OCore.Setup;

public static class Test
{
    static LogLevel LogLevel { get; set; } = LogLevel.Information;

    public static async Task<(IClusterClient, IHost)> LetsGo(
        string applicationTitle = "OCore Test",
        LogLevel? logLevel = null,
        Action<IHostBuilder> hostConfigurationDelegate = null,
        Action<ISiloBuilder> siloConfigurationDelegate = null,
        Action<IWebHostBuilder> webBuilderConfigurationDelegate = null,
        Action<HostBuilderContext, IServiceCollection> serviceConfigurationDelegate = null)
    {
        if (logLevel.HasValue)
        {
            LogLevel = logLevel.Value;
        }

        var hostBuilder = new HostBuilder();
        hostBuilder.TestSetup(siloConfigurationDelegate,
            webBuilderConfigurationDelegate,
            applicationTitle);
        
        if (serviceConfigurationDelegate != null)
        {
            hostBuilder.ConfigureServices(serviceConfigurationDelegate);
        }

        hostConfigurationDelegate?.Invoke(hostBuilder);
        var host = hostBuilder.Build();
        var clusterClient = host.Services.GetRequiredService<IClusterClient>();
        await host.StartAsync();

        return (clusterClient, host);
    }

    public static IHostBuilder TestSetup(this IHostBuilder hostBuilder,
        Action<ISiloBuilder> siloConfigurationDelegate = null,
        Action<IWebHostBuilder> webBuilderConfigurationDelegate = null,
        string applicationName = "OCore App Development")
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        hostBuilder.UseConsoleLifetime();

        var services = Services.Discovery.GetAll();
        foreach (var service in services)
        {
            hostBuilder.AddService(service);
        }

        hostBuilder.ConfigureLogging(logging => logging.AddConsole());

        hostBuilder.ConfigureAppConfiguration(builder => { builder.AddConfiguration(configuration); });

        hostBuilder.ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseUrls("http://*:9000");
            webBuilder.UseStartup<DeveloperStartup>();
            webBuilder.UseSetting("ApplicationTitle", applicationName);
            if (webBuilderConfigurationDelegate != null)
            {
                webBuilderConfigurationDelegate(webBuilder);       
            }
        });

        hostBuilder.UseOrleans((hostBuilderContext, siloBuilder) =>
        {
            siloBuilder.UseLocalhostClustering();
            siloBuilder.AddMemoryGrainStorage("PubSubStore");
            siloBuilder.AddMemoryStreams<DefaultMemoryMessageBodySerializer>("BaseStreamProvider");
            siloBuilder.AddMemoryGrainStorageAsDefault();
            siloBuilder.AddOCoreAuthorization();
            siloBuilder.AddOCoreDeveloperDiagnostics();
            siloBuilder.ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel));
            siloConfigurationDelegate?.Invoke(siloBuilder);
        });

        return hostBuilder;
    }
}