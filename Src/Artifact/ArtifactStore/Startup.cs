using System;
using ArtifactStore.Application;
using ArtifactStore.sdk;
using Directory.sdk;
using MessageNet.sdk;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Spin.Common.Middleware;
using Spin.Common.Model;
using Spin.Common.Services;
using Toolbox.Application;
using Toolbox.Extensions;
using Toolbox.Logging;

namespace ArtifactStore
{
    public class Startup
    {
        private const string _policyName = "defaultPolicy";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton<IServiceStatus>(x => new ServiceStatus().SetStatus(ServiceStatusLevel.Ready));
            services.AddArtifactStore();

            services.AddDirectory();
            services.AddMessageHost();
            services.AddMessageControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ArtifactStore - Hierarchical artifact store", Version = "v1" });
            });

            // CORS
            services.AddCors(x => x.AddPolicy(_policyName, builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetPreflightMaxAge(TimeSpan.FromHours(1));
            }));

            services.AddLogging(config =>
            {
                config.AddLoggerBuffer();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Option option)
        {
            if (env.IsDevelopment() || option.Environment.IsLocal())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ArtifactStore v1"));

            app.UseRouting();

            app.UseAuthorization();
            //app.UseMiddleware<ApiKeyMiddleware>(Constants.ApiKeyName, option.ApiKey, new[] { "/api/ping" } /*"/api/ping".ToEnumerable()*/);
            app.UseMiddleware<ApiKeyMiddleware>(Constants.ApiKeyName, option.ApiKey, "/api/ping".ToEnumerable());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.ConfigureDirectory(option.ConfigStore, option.Environment.ToString());
            app.MapMessageControllers();
            app.StartMessageHost(option.HostServiceId);
        }
    }
}