using OCore.Entities.Data;

namespace OCore.Tests.DataEntities;

[GenerateSerializer]
public class AnimalState
{
    [Id(0)] public string? Name { get; set; }
    [Id(1)] public int Age { get; set; }
    [Id(2)] public int CallCount { get; set; }
    [Id(3)] public string? Noise { get; set; }
};

[DataEntity("Animal", dataEntityMethods: DataEntityMethods.All)]
public interface IAnimal : IDataEntity<AnimalState>
{
    Task<string?> MakeNoise();
}

public class Animal : DataEntity<AnimalState>, IAnimal
{
    public Task<string?> MakeNoise()
    {
        return Task.FromResult(State.Noise);
    }
}