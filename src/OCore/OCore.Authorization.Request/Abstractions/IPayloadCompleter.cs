using Orleans;
using System.Threading.Tasks;

namespace OCore.Authorization.Request.Abstractions
{
    public interface IPayloadCompleter
    {
        Task Complete(Request.Payload payload, 
            IClusterClient clusterClient
            );
    }
}
