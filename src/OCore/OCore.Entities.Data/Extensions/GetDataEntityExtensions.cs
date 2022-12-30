using System.Threading.Tasks;
using Orleans;

namespace OCore.Entities.Data.Extensions
{
    public static class Extensions
    {
        public static ValueTask<T> GetDataEntity<T>(this IGrainFactory grainFactory, string key) where T : IDataEntity
        {
            return ValueTask.FromResult(grainFactory.GetGrain<T>(key));
        }

        public static T GetDataEntity<T>(this IGrainFactory grainFactory, string prefix, string identity) where T : IDataEntity
        {
            return grainFactory.GetGrain<T>($"{prefix}:{identity}");
        }
    }
}
