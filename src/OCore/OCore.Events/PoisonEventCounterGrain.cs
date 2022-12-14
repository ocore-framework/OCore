using System;
using System.Collections.Generic;
using System.Text;
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
