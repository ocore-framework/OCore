using OCore.Services;
using System;
using System.Threading.Tasks;

namespace OCore.Authorization
{
    [Service("OCore.TenantService")]
    public interface ITenantService : IService
    {
        Task<Guid> AddAcount(string accountId);

        Task<string> GetTenantAccount(string accountId);
    }
}
