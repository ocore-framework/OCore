using OCore.Services;
using OCore.Testing.Fixtures;
using OCore.Testing.Host;
using OCore.Tests.Authorization.Services;
using OCore.Tests.Seeders.Authorization;

namespace OCore.Tests.Authorization;

public class AuthorizationTests : FullHost<AuthorizationSeeder>
{
    public AuthorizationTests(FullHostFixture<AuthorizationSeeder> fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task TestOpenEndpointWithoutCredentials()
    {
        var ats = ClusterClient.GetService<IAuthorizationTestService>();

        await ats.Open();
    }
    
    
    [Fact]
    public async Task TestClosedEndpointWithoutCredentials()
    {
        var ats = ClusterClient.GetService<IAuthorizationTestService>();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await ats.Closed());
    }

    [Fact]
    public async Task TestClosedEndpointWithCredentials()
    {
        var ats = ClusterClient.GetService<IAuthorizationTestService>();

        await ats.Closed();
    }
}