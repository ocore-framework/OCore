using System.Net;
using OCore.Testing.Fixtures;
using OCore.Testing.Host;
using OCore.Tests.Seeders.Zoo;

namespace OCore.Tests;

public class DevelopmentTests : FullHost<ZooSeeder>
{
    
    public DevelopmentTests(FullHostFixture<ZooSeeder> fixture) : base(fixture)
    {
    }
    
    [Fact]
    public async Task Test404()
    {
        // Make GET-request to seeded entity
        HttpResponseMessage response200 = await HttpClient.GetAsync("/data/Animal/Dog");
        Assert.Equal(HttpStatusCode.OK, response200.StatusCode);
        
        // Make a GET-request to a non-existing endpoint
        HttpResponseMessage response404 = await HttpClient.GetAsync("/data/Animal/Dig");
        Assert.Equal(HttpStatusCode.NotFound, response404.StatusCode);
    }
}