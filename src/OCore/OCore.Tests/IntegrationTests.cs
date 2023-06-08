using OCore.Entities.Data.Extensions;
using OCore.Tests.DataEntities;
using OCore.Tests.Fixtures;
using OCore.Tests.Host;
using OCore.Tests.Seeders.Zoo;

namespace OCore.Tests;

public class IntegrationTests : FullHost
{
    
    public IntegrationTests(FullHostFixture fixture) : base(fixture)
    {
        Seed(ZooSeeder.Seed);
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