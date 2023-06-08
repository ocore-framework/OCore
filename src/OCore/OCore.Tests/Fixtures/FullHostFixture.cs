using OCore.Tests.Abstractions;

namespace OCore.Tests.Fixtures;

public class FullHostFixture : IAsyncLifetime
{
    public int Port { get; set; }

    public IHost? Host { get; private set; }

    public IClusterClient? ClusterClient { get; private set; }

    private bool _isSeeded = false;

    private object _seedLock = new();

    public void Seed(Func<IClusterClient, Task> seedFunc)
    {
        lock (_seedLock)
        {
            if (_isSeeded == true) return;
            seedFunc(ClusterClient!).Wait();
            _isSeeded = true;
        }
    }

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