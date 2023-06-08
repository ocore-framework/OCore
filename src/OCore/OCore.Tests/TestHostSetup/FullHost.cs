namespace OCore.Tests.TestHostSetup;

public abstract class FullHost : ITestableHost
{
    public IHost Host { get; init; }

    protected FullHost()
    {
        var hostBuilder = new HostBuilder();
        Host = hostBuilder.Build();
    }
}