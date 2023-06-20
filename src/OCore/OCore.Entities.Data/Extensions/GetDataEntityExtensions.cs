using System;
using System.Threading.Tasks;
using Orleans;

namespace OCore.Entities.Data.Extensions
{
    public static class Extensions
    {
        public static T GetDataEntity<T>(this IGrainFactory grainFactory, string key) where T : IDataEntity
        {
            return grainFactory.GetGrain<T>(key);
        }

        public static T GetDataEntity<T>(this IGrainFactory grainFactory, string prefix, string identity) where T : IDataEntity
        {
            return grainFactory.GetGrain<T>($"{prefix}:{identity}");
        }

        public static T GetDataEntity<T>(this IGrainFactory grainFactory, Guid key) where T : IDataEntity
        {
            return grainFactory.GetGrain<T>(key.ToString());
        }
        
        public static T GetDataEntity<T>(this IGrainFactory grainFactory, string prefix, Guid identity) where T : IDataEntity
        {
            return grainFactory.GetGrain<T>($"{prefix}:{identity.ToString()}");
        }
    }
}
