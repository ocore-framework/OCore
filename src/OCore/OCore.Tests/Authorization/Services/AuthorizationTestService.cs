using OCore.Authorization;
using OCore.Services;

namespace OCore.Tests.Authorization.Services;

[Service("AuthorizationTestService")]
public interface IAuthorizationTestService : IService
{
    Task Open();

    [Authorize()]
    Task Closed();
}

public class AuthorizationTestService : Service, IAuthorizationTestService
{
    public Task Open()
    {
        return Task.CompletedTask;
    }

    [Authorize()]
    public Task Closed()
    {
        return Task.CompletedTask;
    }
}