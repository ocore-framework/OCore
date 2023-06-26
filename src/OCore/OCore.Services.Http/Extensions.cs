using Microsoft.Extensions.DependencyInjection;
using OCore.Services.Http;

namespace OCore.Services
{
    public static class Extensions
    {
        public static IServiceCollection AddServiceRouter(this IServiceCollection services)
        {
            return services
                .AddSingleton<ServiceRouter>();
        }
    }
}
