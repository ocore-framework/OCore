using Orleans;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OCore.Authorization
{
    public interface IAccountRoles : IGrainWithStringKey
    {
        Task AddRole(string role);

        Task RemoveRole(string role);

        Task<List<string>> GetRoles();
    }
}
