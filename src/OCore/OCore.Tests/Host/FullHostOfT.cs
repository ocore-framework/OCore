using OCore.Tests.Abstractions;
using OCore.Tests.Fixtures;

namespace OCore.Tests.Host;

public class FullHost<T> : IClassFixture<FullHostFixture<T>>
    where T : ISeeder, new()
{
    FullHostFixture<T> _fixture;

    protected IClusterClient ClusterClient => _fixture.ClusterClient!;

    protected IHost Host => _fixture.Host!;

    protected int PortNumber => _fixture.Port;

    protected string BaseUrl => $"http://localhost:{PortNumber}";

    protected readonly HttpClient _httpClient = new();
    
    public FullHost(FullHostFixture<T> fixture) 
    {
        _fixture = fixture;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }
}