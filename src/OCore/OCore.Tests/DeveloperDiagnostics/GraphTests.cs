using System.Text;
using System.Text.Json;
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
    
    [Fact]
    public async Task ShoutTest()
    {
        var response = await HttpClient.PostAsync("/services/Greeter/SayHelloTo", new StringContent("[\"OCore\"]", Encoding.UTF8, "application/json"));
        var result = await response.Content.ReadAsStringAsync();
        var graph = ClusterClient.GetService<IGreeterService>();
        //var result = await graph.ShoutHelloTo("OCore");
        Assert.Equal("Hello, OCORE!", result);
        
        
    }
}