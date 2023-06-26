using System.Threading.Tasks;

namespace OCore.Events
{
    public class PoisonEventCounterGrain : IPoisonEventCounter
    {
        int count = 1;

        public Task<int> Handle()
        {
            return Task.FromResult(count++);
        }
    }
}
