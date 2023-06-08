using System.Net;
using System.Text;
using OCore.Entities.Data.Extensions;
using OCore.Tests.DataEntities;
using OCore.Tests.Fixtures;
using OCore.Tests.Host;
using OCore.Tests.Seeders.Zoo;

namespace OCore.Tests.Http.DataEntities;

public class DataEntityTests : FullHost
{
    public DataEntityTests(FullHostFixture fixture) : base(fixture)
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
    
    [Fact]
    public async Task Test200()
    {
        // Make a GET-request to a non-existing endpoint
        HttpResponseMessage response = await _httpClient.GetAsync("/data/Animal/Dog");
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Rover", body);
    }

    [Fact]
    public async Task TestPatch()
    {
        var hound = ClusterClient.GetDataEntity<IAnimal>("Hound");
        await hound.Create(new AnimalState
                {
                    Age = 4,
                    Name = "Beast",
                    Noise = "WOOF",
                    CallCount = 0
                });
        
        var response = await _httpClient.PatchAsync("/data/Animal/Hound2", new StringContent("{\"Noise\": \"Woof\"}", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        
    }
}