using OCore.Testing.Fixtures;
using OCore.Testing.Host;
using OCore.Tests.Seeders.Zoo;

namespace OCore.Tests.Http.Hateoas;

public class HateoasTests : FullHost<ZooSeeder>
{
    public HateoasTests(FullHostFixture<ZooSeeder> fixture) : base(fixture)
    {
    }
}