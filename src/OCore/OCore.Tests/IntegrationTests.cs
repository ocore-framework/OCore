using OCore.Entities.Data.Extensions;
using OCore.Tests.DataEntities;
using OCore.Tests.Fixtures;
using OCore.Tests.Seeders.Zoo;

namespace OCore.Tests;

public class IntegrationTests : IClassFixture<FullHostFixture>
{
    FullHostFixture _fixture;

    private IClusterClient ClusterClient => _fixture.ClusterClient!;
    private IHost Host => _fixture.Host!;
    
    
    public IntegrationTests(FullHostFixture fixture)
    {
        _fixture = fixture;
        ClusterClient.Seed().Wait();
    }

    [Fact]
    public async Task Test1()
    {
        var cat = ClusterClient.GetDataEntity<IAnimal>("Cat");
        var dog = ClusterClient.GetDataEntity<IAnimal>("Dog");

        
        var catNoise = await cat.MakeNoise();
        var dogNoise = await dog.MakeNoise();
        
        Assert.Equal("Meow", catNoise);
        Assert.Equal("Woof", dogNoise);
    }
}