﻿using OCore.Testing.Fixtures;
using OCore.Testing.Host;
using OCore.Tests.Seeders.Zoo;

namespace OCore.Tests.Http.Services;

public class ServiceTests : FullHost
{
    public ServiceTests(FullHostFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Test200()
    {
        var response = await HttpClient.PostAsJsonAsync("/services/SayHelloTo", new string[]
        {
            "OCore"
        });
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Hello, OCore!", content);
    }
}