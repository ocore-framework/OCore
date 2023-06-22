using OCore.Authorization;
using OCore.Services;

namespace OCore.Resources;

[Service("OCore.Resource")]
public interface IResourceService : IService
{
    [Authorize]
    Task<List<Resource>> GetResources();

    Task<List<AccessDescription>> GetAccessDescriptions(string resource);

    Task<IEnumerable<AccountAccessDescription>> GetAccountResources(IEnumerable<string> roles);
}