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
        // Make a GET-request to a non-existing endpoint
        HttpResponseMessage response = await _httpClient.GetAsync("/data/Animal/Dig");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}