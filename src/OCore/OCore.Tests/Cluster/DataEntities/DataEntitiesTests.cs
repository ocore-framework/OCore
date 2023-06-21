using OCore.Entities.Data.Extensions;
using OCore.Testing.Fixtures;
using OCore.Testing.Host;
using OCore.Tests.DataEntities;

namespace OCore.Tests.Cluster.DataEntities;

public class DataEntitiesTests : FullHost
{
    public DataEntitiesTests(FullHostFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task TestCreateExists()
    {
        var entity = ClusterClient.GetDataEntity<IAnimal>("Doggie");
        await entity.Create(new AnimalState
        {
            Name = "Doggie",
            Age = 5
        });
        var exists = await entity.Exists();
        
        Assert.True(exists);
    }
}