using OCore.Entities.Data;
using OCore.Http.Hateoas;
using OCore.Http.Hateoas.Attributes;

namespace OCore.Tests.DataEntities;

[GenerateSerializer]
public class AnimalState
{
    [Id(0)] public string? Name { get; set; }
    [Id(1)] public int Age { get; set; }
    [Id(2)] public int CallCount { get; set; }
    [Id(3)] public string? Noise { get; set; }
    
    [Id(4)] public List<HateoasLink> Links { get; set; } = new(); 
};

/// <summary>
/// A beautiful animal.
/// </summary>
[DataEntity("Animal", dataEntityMethods: DataEntityMethods.All)]
public interface IAnimal : IDataEntity<AnimalState>
{
    Task<string?> MakeNoise();
}

public class Animal : DataEntity<AnimalState>, IAnimal
{
    [HateoasGuard("DELETE")]
    public bool CanDelete => false;

    [HateoasGuard(nameof(IAnimal.MakeNoise))]
    public bool CanMakeNoise => false;
    
    public Task<string?> MakeNoise()
    {
        return Task.FromResult(State.Noise);
    }

    public override Task<AnimalState> Read()
    {
        State.Links = this.GetHateoasLinks().ToList();
        return base.Read();
    }
}