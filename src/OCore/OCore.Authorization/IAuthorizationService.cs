using OCore.Services;
using System.Threading.Tasks;

namespace OCore.Authorization
{
    [Service("OCore.Authorization")]
    public interface IAuthorizationService : IService
    {
        
        [Authorize]
        Task<string> Hello();


    }
}
