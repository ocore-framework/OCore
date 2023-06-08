using OCore.Tests.Abstractions;

namespace OCore.Tests.Fixtures;

public class FullHostFixture<TSeeder> : IAsyncLifetime
    where TSeeder : ISeeder, new()
{
    public int Port { get; set; }

    public IHost? Host { get; protected set; }

    public IClusterClient? ClusterClient { get; protected set; }
    
    public async Task InitializeAsync()
    {
        int counter = 0;
        while (true)
        {
            try
            {
                Port = new Random().Next(10000, 20000);
                (ClusterClient, Host) = await Setup.Test.LetsGo(webBuilderConfigurationDelegate: (webHostBuilder) =>
                {
                    webHostBuilder.UseUrls($"http://localhost:{Port}");
                });
                break;
            }
            catch (IOException ex) when (ex.Message.Contains("address already in use"))
            {
                counter++;
                if (counter > 10) throw;
            }
        }
        var seeder = new TSeeder();
        await seeder.Seed(ClusterClient);
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