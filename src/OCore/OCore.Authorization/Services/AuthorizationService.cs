using OCore.Services;
using System.Threading.Tasks;

namespace OCore.Authorization.Services
{
    public class AuthorizationService : Service, IAuthorizationService
    {
        public Task<string> Hello()
        {
            return Task.FromResult("Hello from Authorization service");
        }
    }
}
