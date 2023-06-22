using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
//using OCore.Dashboard;
using OCore.DefaultSetup;
using OCore.Diagnostics.Middleware;
using OCore.Http.Middleware;

namespace OCore.Setup
{
    public class DeveloperStartup
    {
        readonly IConfiguration configuration;

        public DeveloperStartup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDefaultOCore();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseMiddleware<HttpContextMiddleware>();
            app.UseMiddleware<CorrelationIdProviderMiddleware>();
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseRouting();
            var appTitle = configuration.GetValue<string>("ApplicationTitle");
            app.UseDefaultOCore(appTitle,
                openApiInternalPrefixes: new[]
                {
                    "OCore.CorrelationIdRecorder",
                    "OCore.CorrelationIdRegistry"
                });
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/api-docs", appTitle); });
            //app.UseOCoreDashboard(env);
        }
    }
}