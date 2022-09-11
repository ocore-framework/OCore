using OCore.Entities.Data;
using OCore.Services;
using Orleans;

await OCore.Setup.Developer.LetsGo("Hello World");

[Service("HelloWorld")]
public interface IHelloWorldService : IService
{
    Task<string> SayHelloTo(string name);
    Task<string> ShoutHelloTo(string name);
}

public class HelloWorldService : Service, IHelloWorldService
{
    readonly ICapitalizationService capitalizationService;

    public HelloWorldService(ICapitalizationService capitalizationService)
    {
        this.capitalizationService = capitalizationService;
    }

    public Task<string> SayHelloTo(string name) => Task.FromResult($"Hello, {name}!");

    public async Task<string> ShoutHelloTo(string name)
        => await SayHelloTo(await capitalizationService.Capitalize(name));
}

[Service("Capitalization")]
public interface ICapitalizationService : IService
{
    Task<string> Capitalize(string name);
}

public class NameCapitalizationService : Service, ICapitalizationService
{
    public Task<string> Capitalize(string name) => Task.FromResult(name.ToUpper());
}

[GenerateSerializer]
public class CalculatorState
{
    [Id(0)] public decimal Value { get; set; }
}

[DataEntity("Calculator", dataEntityMethods: DataEntityMethods.All)]
public interface ICalculator : IDataEntity<CalculatorState>
{
    Task<decimal> Add(decimal value);
}

public class Calculator : DataEntity<CalculatorState>, ICalculator
{
    public async Task<decimal> Add(decimal value)
    {
        State.Value += value;
        await WriteStateAsync();
        return State.Value;
    }
}
