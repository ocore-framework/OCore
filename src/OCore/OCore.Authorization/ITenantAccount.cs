using Orleans;
using System.Threading.Tasks;

namespace OCore.Authorization
{
    /// <summary>
    /// This is keyed on base account ID with the tenant ID as extension
    /// </summary>
    public interface ITenantAccount : IGrainWithStringKey
    {
        Task Create(string accountId);

        Task<string> Get();

        Task Delete();
    }
}
