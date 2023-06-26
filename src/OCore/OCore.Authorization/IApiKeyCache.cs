using Orleans;
using System.Threading.Tasks;

namespace OCore.Authorization
{
    public interface IApiKeyCache : IGrainWithStringKey
    {
        Task<ApiKeyState> GetApiKey();
    }
}
