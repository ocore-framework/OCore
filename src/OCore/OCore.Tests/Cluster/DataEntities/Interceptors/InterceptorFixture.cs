using OCore.Entities.Data;
using OCore.Testing.Fixtures;
using OCore.Tests.DataEntities;

namespace OCore.Tests.Cluster.DataEntities.Interceptors;

public class InterceptorFixture : FullHostFixture
{
    
    protected override Task SetupServer()
    {
        ServiceCollectionConfigurationDelegate = (_, collection) =>
        {
            collection.AddSingleton<IInterceptor<AnimalState>, StoreInterceptor>();
        };
        return base.SetupServer();
    }
}