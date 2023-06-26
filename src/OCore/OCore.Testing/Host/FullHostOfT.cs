using Microsoft.Extensions.Hosting;
using OCore.Testing.Abstractions;
using OCore.Testing.Fixtures;

namespace OCore.Testing.Host;

public class FullHost<T> : IClassFixture<FullHostFixture<T>>
    where T : ISeeder, new()
{
    FullHostFixture<T> _fixture;

    protected IClusterClient ClusterClient => _fixture.ClusterClient!;

    protected IHost Host => _fixture.Host!;

    protected int PortNumber => _fixture.Port;

    protected string BaseUrl => $"http://localhost:{PortNumber}";

    private readonly HttpClient _httpClient = new();
    
    protected HttpClient HttpClient => _httpClient;
    
    public FullHost(FullHostFixture<T> fixture) 
    {
        _fixture = fixture;
        HttpClient.BaseAddress = new Uri(BaseUrl);
    }
}