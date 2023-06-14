﻿using System.Text;
using OCore.DiagnosticsServices;
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

    [Fact(Skip = "Will fix")]
    public async Task ShoutTest()
    {
        // Why doesn't this work?
        var response = await HttpClient.PostAsJsonAsync("/services/Greeter/SayHelloTo", new object[] { "OCore" });
        
        var body = await response.Content.ReadAsStringAsync();

        var correlationIdString = response.Headers.GetValues("CorrelationId").FirstOrDefault();
        var correlationId = Guid.Parse(correlationIdString!);

        var graph = ClusterClient.GetService<IGreeterService>();
        var result = await graph.ShoutHelloTo("OCore");
        Assert.Equal("Hello, OCORE!", result);
    }
}