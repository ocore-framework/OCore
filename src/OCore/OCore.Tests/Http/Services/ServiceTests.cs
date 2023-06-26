using System.Text;
using OCore.Testing.Fixtures;
using OCore.Testing.Host;

namespace OCore.Tests.Http.Services;

public class ServiceTests : FullHost
{
    public ServiceTests(FullHostFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Test200()
    {
        var response = await HttpClient.PostAsync("services/Greeter/SayHelloTo", new StringContent("[\"OCore\"]", Encoding.UTF8, "application/json"));
        //var response = await HttpClient.PostAsJsonAsync("services/Greeter/SayHelloTo", "\"OCore\"");

        //await Task.Delay(-1);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Hello, OCore!", content);
    }
}