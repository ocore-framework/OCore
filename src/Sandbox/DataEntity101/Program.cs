using System.Reflection;
using OCore.Diagnostics.C4;
using OCore.Entities.Data;
using OCore.Services;

var diagram = Assembly.GetExecutingAssembly().GenerateC4Model("DataEntity101");

await OCore.Setup.Developer.LetsGo("DataEntities101");

// DataEntities
/// <summary>
/// A beautiful animal.
/// </summary>
[DataEntity("Animal", dataEntityMethods: DataEntityMethods.All)]
public interface IAnimal : IDataEntity<AnimalState>
{
    /// <summary>
    /// What noise does this animal make?
    /// </summary>
    /// <returns></returns>
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
public class HomeState
{
    [Id(0)] public string Description { get; set; } = String.Empty;
}

[DataEntity("Home", dataEntityMethods: DataEntityMethods.All)]
public interface IHome : IDataEntity<HomeState>
{
    
}

public class Home : DataEntity<HomeState>, IHome
{
    
}

[GenerateSerializer]
public class AnimalState
{
    [Id(0)] public string Name { get; set; } = string.Empty;
    
    [Id(1)] public int Age { get; set; }

    [Id(2)] public string Noise { get; set; } = string.Empty;
}

[GenerateSerializer]
public class ShoutRequest
{
    [Id(0)] public string Name { get; set; } = string.Empty;

    [Id(1)] public int Times { get; set; } = 3;
}

/// <summary>
/// Some general greeter functionality.
/// </summary>
[Service("Greeter")]
public interface IGreeterService : IService
{
    /// <summary>
    /// Just a plain ol' greeting.
    /// </summary>
    /// <param name="name">Who we're addressinmg</param>
    /// <returns></returns>
    Task<string> SayHelloTo(string name);
    Task<string> ShoutHelloTo(string name);

    Task<string> ComplexShout(ShoutRequest request);
}

/// <inhericdoc />
public class GreeterService: Service, IGreeterService
{
    public Task<string> SayHelloTo(string name)
    {
        return Task.FromResult($"Hello, {name}!");
    }

    public async Task<string> ShoutHelloTo(string name)
    {
        if (name == "OCore")
        {
            throw new ArgumentException("OCore is not a person!", nameof(name));
        }
        var testGrain = GrainFactory.GetGrain<IStringFun>("test");
        var upperName = await testGrain.Capitalize(name);
        return $"Hello, {upperName}!";
    }

    public Task<string> ComplexShout(ShoutRequest request)
    {
        // Shout the name back to the user, n times
        var names = new List<string>();
        for (var i = 0; i < request.Times; i++)
        {
            names.Add(request.Name);
        }
        return Task.FromResult($"Hello, {string.Join(", ", names)}!");
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