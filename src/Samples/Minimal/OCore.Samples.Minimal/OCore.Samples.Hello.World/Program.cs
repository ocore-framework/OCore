using OCore.Services;
await OCore.Setup.Developer.LetsGo("Hello World");

[Service("HelloWorld")]
public interface IHelloWorldService : IService
{
    Task<string> SayHelloTo(string name);
    Task<string> ShoutHelloTo(string name);
}

public class HelloWorldService : Service, IHelloWorldService
{
    readonly INameCapitalizationService nameCapitalizationService;

    public HelloWorldService(INameCapitalizationService nameCapitalizationService)
    {
        this.nameCapitalizationService = nameCapitalizationService;
    }

    public Task<string> SayHelloTo(string name) => Task.FromResult($"Hello, {name}!");

    public async Task<string> ShoutHelloTo(string name)
        => await SayHelloTo(await nameCapitalizationService.Capitalize(name));
}

[Service("Capitalization")]
public interface INameCapitalizationService : IService
{
    Task<string> Capitalize(string name);
}

public class NameCapitalizationService : Service, INameCapitalizationService
{
    public Task<string> Capitalize(string name) => Task.FromResult(name.ToUpper());    
}
