using System.Net;
using OCore.Tests.Fixtures;
using OCore.Tests.Host;
using OCore.Tests.Seeders.Zoo;

namespace OCore.Tests;

public class DevelopmentTests : FullHost
{
    
    public DevelopmentTests(FullHostFixture fixture) : base(fixture)
    {
        Seed(ZooSeeder.Seed);
    }
    
    [Fact]
    public async Task Test404()
    {
        // Make a GET-request to a non-existing endpoint
        HttpResponseMessage response = await _httpClient.GetAsync("/data/Animal/Dig");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}