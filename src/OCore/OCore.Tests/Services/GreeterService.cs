using OCore.Services;

//namespace OCore.Tests.Services;

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