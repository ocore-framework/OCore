using OCore.Entities.Data;
using OCore.Services;

await OCore.Setup.Developer.LetsGo("DataEntities101");

// DataEntities

[DataEntity("Animal", dataEntityMethods: DataEntityMethods.All)]
public interface IAnimal : IDataEntity<AnimalState>
{
    Task<string> MakeNoise();
}

public class Animal : DataEntity<AnimalState>, IAnimal
{
    public Task<string> MakeNoise()
    {
        return Task.FromResult($"{State.Name} ({State.Age}) says {State.Noise}!");
    }
}

[GenerateSerializer]
public class AnimalState
{
    [Id(0)] public string Name { get; set; } = string.Empty;
    
    [Id(1)] public int Age { get; set; }

    [Id(2)] public string Noise { get; set; } = string.Empty;
}

[Service("Greeter")]
public interface IGreeterService : IService
{
    Task<string> SayHelloTo(string name);
    Task<string> ShoutHelloTo(string name);
}

public class GreeterService: Service, IGreeterService
{
    public Task<string> SayHelloTo(string name)
    {
        return Task.FromResult($"Hello, {name}!");
    }

    public async Task<string> ShoutHelloTo(string name)
    {
        var testGrain = GrainFactory.GetGrain<IStringFun>("test");
        var upperName = await testGrain.Capitalize(name);
        return $"Hello, {upperName}!";
    }

    public async Task<string> ShoutHelloBackwards(string name)
    {
        var testGrain = GrainFactory.GetGrain<IStringFun>("test");
        var upperName = await testGrain.Capitalize(name);
        var reversedName = await testGrain.ReverseString(upperName);
        return $"Hello, {reversedName}!";
    }
}

public interface IStringFun : IGrainWithStringKey
{
    Task<string> ReverseString(string input);

    Task<string> Capitalize(string input);
}

public class StringFun : Grain, IStringFun
{
    private int callCount = 0;
    
    static string Reverse(string s)
    {
        char[] charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }
    
    public Task<string> ReverseString(string input)
    {
        callCount++;
        return Task.FromResult(Reverse(input));
    }

    public Task<string> Capitalize(string input)
    {
        callCount++;
        return Task.FromResult(input.ToUpper());
    }
}