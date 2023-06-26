using Orleans;
using System.Threading.Tasks;

namespace OCore.Authorization
{
    public interface ISystemSetup : IGrainWithIntegerKey
    {
        Task<bool> IsSystemSetup();
        Task SetupSystem();
    }
}
