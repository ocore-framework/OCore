using Microsoft.Extensions.DependencyInjection;

namespace OCore.Entities.Data.SignalR;

public static class Extensions
{
    public static IServiceCollection AddSignalRDataEntityHub(this IServiceCollection services)
    {
        return services
            .AddSingleton<DataEntityHub>();
    }
}
