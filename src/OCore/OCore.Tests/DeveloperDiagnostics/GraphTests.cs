using OCore.Services;
using OCore.Testing.Abstractions;
using OCore.Testing.Fixtures;
using OCore.Testing.Host;

namespace OCore.Tests.DeveloperDiagnostics;

public class GraphTestsSeeder : ISeeder
{
    public Task Seed(IClusterClient clusterClient)
    {
        return Task.CompletedTask;
    }
}

public class GraphTests : FullHost<GraphTestsSeeder>
{
    public GraphTests(FullHostFixture<GraphTestsSeeder> fixture) : base(fixture)
    {
    }

    [Fact(Skip="We'll get back to this when we get services to work in the test environment")]
    public async Task ShoutTest()
    {
        Console.WriteLine(HttpClient.BaseAddress);
        // Why doesn't this work?
        var response = await HttpClient.PostAsJsonAsync("/services/Greeter/SayHelloTo", new object[] { "OCore" });
        
        var body = await response.Content.ReadAsStringAsync();

        //await Task.Delay(1000000);

        var correlationIdString = response.Headers.GetValues("correlationid").FirstOrDefault();
        var correlationId = Guid.Parse(correlationIdString!);

        var graph = ClusterClient.GetService<IGreeterService>();
        var result = await graph.ShoutHelloTo("OCore");
        Assert.Equal("Hello, OCORE!", result);
    }
}