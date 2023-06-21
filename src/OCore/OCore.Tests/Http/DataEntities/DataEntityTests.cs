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
    public async Task Test200()
    {
        // Make a GET-request to a seeded endpoint
        HttpResponseMessage response = await HttpClient.GetAsync("/data/Animal/Dog");
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Rover", body);
    }
    
    [Fact]
    public async Task Test200Fanout()
    {
        // Make a GET-request to a seeded endpoint
        HttpResponseMessage response = await HttpClient.GetAsync("/data/Animal/Dog,Cat");
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Rover", body);
        Assert.Contains("Fluffy", body);
    }
    
    [Fact]
    public async Task Test404()
    {
        // Make a GET-request to a non-existing endpoint
        HttpResponseMessage response = await HttpClient.GetAsync("/data/Animal/Dig");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task TestDelete()
    {
        var postResponse = await HttpClient.PostAsJsonAsync("/data/Animal/Tiger", new AnimalState()
        {
            Name = "Tiger",
            Age = 3,
            Noise = "ROAR!",
        });
        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        
        var animalState = await HttpClient.GetFromJsonAsync<AnimalState>("/data/Animal/Tiger");
        
        Assert.NotNull(animalState);
        Assert.Equal("Tiger", animalState.Name);
        Assert.Equal(3, animalState.Age);
        Assert.Equal("ROAR!", animalState.Noise);

        var deleteResponse = await HttpClient.DeleteAsync("/data/Animal/Tiger");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        
        var getResponse = await HttpClient.GetAsync("/data/Animal/Tiger");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
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
    
    /// <summary>
    /// Trying to create a data entity with the same ID twice should fail with 409 CONFLICT.
    /// </summary>
    [Fact]
    public async Task TestPostDuplicate()
    {
        var postResponse = await HttpClient.PostAsJsonAsync("/data/Animal/Arfwing", new StringContent("{\"Noise\": \"Yipyip!\"}", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        
        postResponse = await HttpClient.PostAsJsonAsync("/data/Animal/Arfwing", new StringContent("{\"Noise\": \"Yipyip!\"}", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Conflict, postResponse.StatusCode);
    }
    
    [Fact]
    public async Task TestPostSuccess()
    {
        var postResponse = await HttpClient.PostAsJsonAsync("/data/Animal/Bonnie", new AnimalState
        {
            Name = "Bonnie",
            Age = 3,
            Noise = "Yipyip!",
        });
        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        
        var getResponse = await HttpClient.GetAsync("/data/Animal/Bonnie");
        var body = await getResponse.Content.ReadAsStringAsync();
        Assert.Contains("Yipyip!", body);
        Assert.Contains("Bonnie", body);
    }
    
    [Fact]
    public async Task TestPostSuccessDeserializeJson()
    {
        var postResponse = await HttpClient.PostAsJsonAsync("/data/Animal/Wabwab", new AnimalState()
        {
            Name = "Wabwab",
            Age = 3,
            Noise = "Yipyip!",
        });
        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        
        var animalState = await HttpClient.GetFromJsonAsync<AnimalState>("/data/Animal/Wabwab");
        
        Assert.NotNull(animalState);
        Assert.Equal("Wabwab", animalState.Name);
        Assert.Equal(3, animalState.Age);
        Assert.Equal("Yipyip!", animalState.Noise);
    }

    [Fact]
    public async Task TestCommand()
    {
        var postResponse = await HttpClient.PostAsJsonAsync("/data/Animal/Largo", new AnimalState()
        {
            Name = "Largo",
            Age = 3,
            Noise = "Yipyip!",
        });
        
        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        
        postResponse = await HttpClient.PostAsync("/data/Animal/Largo/MakeNoise", null);
        var body = await postResponse.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        Assert.Equal("\"Yipyip!\"", body);
    }
}