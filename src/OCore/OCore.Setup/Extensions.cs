using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OCore.Diagnostics;
using OCore.Entities.Data.Http;
using OCore.Entities.Data.SignalR;
using OCore.Http.OpenApi;
using OCore.Services;
using OCore.Services.Http;

namespace OCore.DefaultSetup
{
    public static class Extensions
    {
        public static IServiceCollection AddDefaultOCore(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSignalR();
            serviceCollection.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });
            return serviceCollection
                .AddServiceRouter()
                .AddDiagnosticIncomingGrainCallFilter();
        }

        public static void UseDefaultOCore(this IApplicationBuilder app,
            string appTitle = "OCore App Development",
            string version = "Development",
            bool stripInternal = true,
            string[] openApiInternalPrefixes = null)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapServices("services");
                endpoints.MapDataEntities("data");
                endpoints.MapHub<DataEntityHub>("/data-streaming");
                endpoints.MapDeveloperOpenApi("api-docs",
                    appTitle,
                    version,
                    stripInternal,
                    openApiInternalPrefixes);
            });
        }
    }
}