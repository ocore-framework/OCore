using Orleans;
using System.Threading.Tasks;

namespace OCore.Events
{
    public interface IEventAggregator : IGrainWithIntegerKey
    {
        Task Raise<T>(T @event, string streamNameSuffix = null);
    }
}
