using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using System;

namespace OCore.Services
{
    public static class Extensions
    {
        public static T GetService<T>(this IGrainFactory grainFactory) where T : IGrainWithIntegerKey
        {
            return grainFactory.GetGrain<T>(0);
        }

        public static IHostBuilder AddService(this IHostBuilder hostBuilder, Type serviceType) 
        {
            hostBuilder.ConfigureServices((context, services) =>
            {                
                services.AddSingleton(sp => {
                    var grainFactory = sp.GetRequiredService<IGrainFactory>();
                    return grainFactory.GetGrain(serviceType, 0);
                });             
            });
            return hostBuilder;
        }
    }
}
