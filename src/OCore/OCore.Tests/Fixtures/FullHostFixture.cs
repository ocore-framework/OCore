using OCore.Tests.Abstractions;

namespace OCore.Tests.Fixtures;

public class FullHostFixture : IAsyncLifetime
{
    public IHost? Host { get; private set; }

    public IClusterClient? ClusterClient { get;  private set; }
    

    public async Task InitializeAsync()
    {
        (ClusterClient, Host) = await Setup.Test.LetsGo();
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
