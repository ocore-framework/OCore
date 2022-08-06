﻿using OCore.Services;
await OCore.Setup.DeveloperExtensions.LetsGo();

namespace HelloWorld
{
    [Service("HelloWorld")]
    public interface IHelloWorldService : IService
    {
        Task<string> SayHelloTo(string name);

        Task<string> ShoutHelloTo(string name);
    }

    public class HelloWorldService : Service, IHelloWorldService
    {
        public Task<string> SayHelloTo(string name) 
            => Task.FromResult($"Hello, {name}!");        

        public async Task<string> ShoutHelloTo(string name)
        {
            var capitalizationService = GetService<INameCapitalizationService>();

            var capitalizedName = await capitalizationService.Capitalize(name);

            var helloWorldService = GetService<IHelloWorldService>();

            return await helloWorldService.SayHelloTo(capitalizedName);
        }
    }

    [Service("Capitalization")]
    public interface INameCapitalizationService : IService
    {
        Task<string> Capitalize(string name);
    }

    public class NameCapitalizationService : Service, INameCapitalizationService
    {
        public Task<string> Capitalize(string name)
        {
            throw new ArgumentException(nameof(name));
            return Task.FromResult(name.ToUpper());
        }
    }
}