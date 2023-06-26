using OCore.Services;
using System;
using System.Threading.Tasks;

namespace OCore.Authorization.Services
{
    public class TenantService : Service, ITenantService
    {
        public Task<Guid> AddAcount(string accountId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetTenantAccount(string accountId)
        {
            throw new NotImplementedException();
        }
    }
}
