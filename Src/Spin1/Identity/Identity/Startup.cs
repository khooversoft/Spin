using Identity.Application;
using Identity.sdk;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Spin.Common.Middleware;
using Spin.Common.Model;
using Spin.Common.Services;
using System;
using Toolbox.Application;
using Toolbox.Logging;

namespace Identity
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
            services.AddIdentityService();
            services.AddControllers();
            services.AddSingleton<IServiceStatus>(_ => new ServiceStatus().SetStatus(ServiceStatusLevel.Ready));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity", Version = "v1" });
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
            if (env.IsDevelopment() || option.RunEnvironment.IsLocal())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            app.UseMiddleware<ApiKeyMiddleware>(Constants.ApiKeyName, option.ArtifactStore.ApiKey, new[] { "/api/ping" });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}