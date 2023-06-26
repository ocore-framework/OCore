using OCore.Services;
using System.Threading.Tasks;
using OCore.Authorization.Request;

namespace OCore.Authorization
{
    [Service("OCore.System")]
    public interface ISystemService : IService
    {
        /// <summary>
        /// Setup the system for first use using a Guid token
        /// </summary>
        /// <param name="token">The seed token for the system.</param>
        /// <param name="accountId">The account ID to promote to root.</param>
        /// <returns></returns>
        [Authorize(requirements: Requirements.None)]
        Task Initialize(string token, string accountId);
    }
}
