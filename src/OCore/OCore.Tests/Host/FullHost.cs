﻿using OCore.Tests.Abstractions;
using OCore.Tests.Fixtures;

namespace OCore.Tests.Host;

[Collection("Sequential")]
public class FullHost : IClassFixture<FullHostFixture>
{
    FullHostFixture _fixture;

    protected IClusterClient ClusterClient => _fixture.ClusterClient!;

    protected IHost Host => _fixture.Host!;

    protected int PortNumber => _fixture.Port;

    protected string BaseUrl => $"http://localhost:{PortNumber}";

    protected readonly HttpClient _httpClient = new();
    
    public FullHost(FullHostFixture fixture)
    {
        _fixture = fixture;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }
}