namespace OCore.Tests.Cluster.DataEntities.Interceptors;

public class InterceptorHost : IClassFixture<InterceptorFixture>
{
    InterceptorFixture _fixture;

    protected IClusterClient ClusterClient => _fixture.ClusterClient!;

    protected IHost Host => _fixture.Host!;

    protected int PortNumber => _fixture.Port;

    protected string BaseUrl => $"http://localhost:{PortNumber}";

    private readonly HttpClient _httpClient = new();
    
    protected HttpClient HttpClient => _httpClient;
    
    public InterceptorHost(InterceptorFixture fixture)
    {
        _fixture = fixture;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }
}