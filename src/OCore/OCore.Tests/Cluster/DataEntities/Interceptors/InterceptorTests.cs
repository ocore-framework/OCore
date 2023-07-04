using OCore.Entities.Data;
using OCore.Entities.Data.Extensions;
using OCore.Tests.DataEntities;

namespace OCore.Tests.Cluster.DataEntities.Interceptors;

public class StoreInterceptor : IInterceptor<AnimalState>
{
    public bool Set { get; set; } = false;
    
    public Task OnCreate(AnimalState data)
    {
        Set = true;
        return Task.CompletedTask;
    }

    public Task<AnimalState> OnRead(AnimalState state)
    {
        return Task.FromResult<AnimalState>(new());
    }

    public Task OnUpdate(AnimalState state, AnimalState data)
    {
        return Task.CompletedTask;
    }

    public Task OnPartialUpdate(AnimalState state, AnimalState data, string[] fields)
    {
        return Task.CompletedTask;
    }

    public Task OnUpsert(AnimalState state, AnimalState data)
    {
        return Task.CompletedTask;
    }

    public Task OnDelete(AnimalState state)
    {
        return Task.CompletedTask;
    }

    public Task OnCommit(AnimalState state)
    {
        return Task.CompletedTask;
    }
}

// public static class SetupDI
// {
//     public static void Setup(IServiceCollection collection)
//     {
//         collection.AddSingleton<IInterceptor<AnimalState>, StoreInterceptor>();
//     }
// }

public class InterceptorTests : InterceptorHost
{

    public InterceptorTests(InterceptorFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task TestCreateExists()
    {
        var entity = ClusterClient.GetDataEntity<IAnimal>("Doggie");
        await entity.Create(new AnimalState
        {
            Name = "Doggie",
            Age = 5
        });
        var exists = await entity.Exists();

        var interceptors = ClusterClient.ServiceProvider.GetServices<IInterceptor<AnimalState>>();
        
        Assert.Single(interceptors);
        
        var storeInterceptor = interceptors.First() as StoreInterceptor;
        
        Assert.True(storeInterceptor!.Set);
        Assert.True(exists);
    }
}