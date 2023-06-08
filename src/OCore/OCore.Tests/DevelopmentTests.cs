using OCore.Tests.Fixtures;

namespace OCore.Tests;

public class DevelopmentTests : IClassFixture<FullHostFixture>
{
    FullHostFixture _fixture;

    private IClusterClient ClusterClient => _fixture.ClusterClient!;
    private IHost Host => _fixture.Host!;
    
    
    public DevelopmentTests(FullHostFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task Test()
    {
        ;
    }
}