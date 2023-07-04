using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OCore.Testing.Fixtures;

public class FullHostFixture : IAsyncLifetime
{
    public int Port { get; set; }

    public IHost? Host { get; protected set; }

    public IClusterClient? ClusterClient { get; protected set; }

    protected Action<HostBuilderContext, IServiceCollection>? ServiceCollectionConfigurationDelegate { get; set; }

    public async Task InitializeAsync()
    {
        await SetupServer();
    }

    protected virtual async Task SetupServer()
    {
        int counter = 0;
        while (true)
        {
            try
            {
                Port = new Random().Next(10000, 20000);
                (ClusterClient, Host) =
                    await Setup.Test.LetsGo(
                        webBuilderConfigurationDelegate: (webHostBuilder) =>
                        {
                            webHostBuilder.UseUrls($"http://localhost:{Port}");
                        }, serviceConfigurationDelegate: ServiceCollectionConfigurationDelegate);
                break;
            }
            catch (IOException ex) when (ex.Message.Contains("address already in use"))
            {
                counter++;
                if (counter > 10) throw;
            }
        }
    }

    public async Task DisposeAsync()
    {
        if (Host != null)
        {
            await Host.StopAsync();
            Host.Dispose();
        }
    }
}