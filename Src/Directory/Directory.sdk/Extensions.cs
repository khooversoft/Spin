using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Directory.sdk
{
    public static class Extensions
    {
        public static IServiceCollection AddDirectory(this IServiceCollection serviceCollection)
        {
            serviceCollection.VerifyNotNull(nameof(serviceCollection));

            serviceCollection.AddSingleton<IDirectoryNameService, DirectoryNameService>();
            return serviceCollection;
        }

        public static void ConfigureDirectory(this IApplicationBuilder app, string configStore, string environment)
        {
            app.VerifyNotNull(nameof(app));

            app.ApplicationServices
                .ConfigureDirectory(configStore, environment);
        }

        public static void ConfigureDirectory(this IServiceProvider serviceProvider, string configStore, string environment)
        {
            serviceProvider.VerifyNotNull(nameof(serviceProvider));
            configStore.VerifyNotEmpty(nameof(configStore));
            environment.VerifyNotEmpty(nameof(environment));

            serviceProvider
                .GetRequiredService<IDirectoryNameService>()
                .Load(configStore, environment)
                .SelectDefault(environment);
        }

        public static IConfigurationBuilder AddDirectoryServices(this IConfigurationBuilder configurationBuilder, string? configStore = null, string? environment = null)
        {
            (string ConfigStore, string Environment) env = configurationBuilder.SetEnvironment(configStore, environment);

            var db = new DirectoryNameService()
                .Load(env.ConfigStore, env.Environment)
                .SelectDefault(env.Environment);

            var dict = db.Service.Values.SelectMany(x =>
                new [] {
                    ($"service.{x.ServiceId}.hostUrl", x.HostUrl).ToKeyValuePair(),
                    ($"service.{x.ServiceId}.apiKey", x.ApiKey).ToKeyValuePair()
                    }
                );

            configurationBuilder.AddInMemoryCollection(dict);

            return configurationBuilder;
        }

        public static (string ConfigStore, string Environment) SetEnvironment(this IConfigurationBuilder configurationBuilder, string? configStore = null, string? environment = null)
        {
            IConfiguration config = configurationBuilder.Build();

            configStore ??= config["ConfigStore"];
            environment ??= config["Environment"];

            configStore.VerifyNotEmpty("'ConfigStore' not found in configuration or provided");
            environment.VerifyNotEmpty("'Environment' not found in configuration or provided");

            var dict = new[]
            {
                ("ConfigStore", configStore).ToKeyValuePair(),
                ("Environment", environment).ToKeyValuePair(),
            };

            configurationBuilder.AddInMemoryCollection(dict);

            return (configStore, environment);
        }
    }
}
