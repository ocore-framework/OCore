﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
//using OCore.Dashboard;
using OCore.DefaultSetup;
using OCore.Diagnostics.Middleware;
using OCore.Http.Middleware;

namespace OCore.Setup
{
    public class TestStartup
    {
        readonly IConfiguration configuration;

        public TestStartup(IConfiguration configuration)
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
            app.UseDefaultOCore(appTitle);
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/api-docs", appTitle);
            });
        }
    }
}
