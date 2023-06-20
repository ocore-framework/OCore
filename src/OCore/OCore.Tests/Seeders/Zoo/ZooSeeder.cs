using OCore.Entities.Data.Extensions;
using OCore.Testing.Abstractions;
using OCore.Tests.DataEntities;

namespace OCore.Tests.Seeders.Zoo;

public class ZooSeeder : ISeeder 
{
    public async Task Seed(IClusterClient clusterClient)
    {
        var cat = clusterClient.GetDataEntity<IAnimal>("Cat");
        var dog = clusterClient.GetDataEntity<IAnimal>("Dog");

        await cat.Create(new AnimalState
        {
            Age = 4,
            Name = "Fluffy",
            Noise = "Meow",
            CallCount = 0
        });
        
        await dog.Create(new AnimalState
        {
            Age = 2,
            Name = "Rover",
            Noise = "Woof",
            CallCount = 0
        });
    }
}