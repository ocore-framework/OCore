using Orleans;
using Orleans.Concurrency;

namespace OCore.Services
{
    [StatelessWorker]
    [Reentrant]
    public class Service : Grain
    {
        protected T GetService<T>() where T: IService
        {
            return GrainFactory.GetGrain<T>(0);
        }

    }
}
