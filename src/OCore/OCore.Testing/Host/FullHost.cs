using Microsoft.Extensions.Hosting;
using OCore.Testing.Fixtures;
using Xunit;

namespace OCore.Testing.Host;

public class FullHost : IClassFixture<FullHostFixture>
{
    FullHostFixture _fixture;

    protected IClusterClient ClusterClient => _fixture.ClusterClient!;

    protected IHost Host => _fixture.Host!;

    protected int PortNumber => _fixture.Port;

    protected string BaseUrl => $"http://localhost:{PortNumber}";

    private readonly HttpClient _httpClient = new();
    
    protected HttpClient HttpClient => _httpClient;
    
    public FullHost(FullHostFixture fixture)
    {
        _fixture = fixture;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }
}