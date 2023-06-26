using Orleans.Runtime;

namespace OCore.Core.Extensions
{
    public static class Extensions
    {
        public static Key Key(this IAddressable grain)
        {
            return Core.Key.FromGrain(grain);
        }
    }
}
