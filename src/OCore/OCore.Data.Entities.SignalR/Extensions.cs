using Microsoft.Extensions.DependencyInjection;

namespace OCore.Data.Entities.SignalR;

public static class Extensions
{
    public static IServiceCollection AddSignalRDataEntityHub(this IServiceCollection services)
    {
        return services
            .AddSingleton<DataEntityHub>();
    }
}
