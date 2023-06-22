using System.Net;
using OCore.Testing.Fixtures;
using OCore.Testing.Host;
using OCore.Tests.Seeders.Zoo;

namespace OCore.Tests.Http.CorrelationId;

public class CorrelationIdTests : FullHost<ZooSeeder>
{
    [Fact]
    public async void CheckCorrelationIdMatch()
    {
        using var requestMessage =
            new HttpRequestMessage(HttpMethod.Get, $"/data/Animal/Dog");

        requestMessage.Headers.Add("correlationId", "something");

        var response = await HttpClient.SendAsync(requestMessage);
        
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // Check that the correlationId is in the response header
        Assert.Equal("something", response.Headers.GetValues("correlationId").First());
        Assert.Contains("Rover", body);
    }

    public CorrelationIdTests(FullHostFixture<ZooSeeder> fixture) : base(fixture)
    {
    }
}