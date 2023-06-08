using System.Net;
using System.Text;
using OCore.Entities.Data.Extensions;
using OCore.Tests.DataEntities;
using OCore.Testing.Fixtures;
using OCore.Testing.Host;
using OCore.Tests.Seeders.Zoo;

namespace OCore.Tests.Http.DataEntities;

public class DataEntityTests : FullHost<ZooSeeder>
{
    public DataEntityTests(FullHostFixture<ZooSeeder> fixture) : base(fixture)
    {
    }
    
    [Fact]
    public async Task Test404()
    {
        // Make a GET-request to a non-existing endpoint
        HttpResponseMessage response = await HttpClient.GetAsync("/data/Animal/Dig");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task Test200()
    {
        // Make a GET-request to a non-existing endpoint
        HttpResponseMessage response = await HttpClient.GetAsync("/data/Animal/Dog");
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
        
        var patchResponse = await HttpClient.PatchAsync("/data/Animal/Hound", new StringContent("{\"Noise\": \"WOOOF!!!\"}", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);
        
        var getResponse = await HttpClient.GetAsync("/data/Animal/Hound");
        var body = await getResponse.Content.ReadAsStringAsync();
        Assert.Contains("WOOOF!!!", body);
        Assert.Contains("Beast", body);
    }
}