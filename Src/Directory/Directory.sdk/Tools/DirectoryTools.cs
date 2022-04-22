using Directory.sdk.Client;
using Directory.sdk.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Configuration;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Directory.sdk.Tools;

public static class DirectoryTools
{
    public static DirectoryOption GetDirectoryOption(string configStore, RunEnvironment runEnvironment)
    {
        IReadOnlyList<KeyValuePair<string, string>> appConfig = new DirectoryOption().GetConfigurationValues();

        string configFile = $"{configStore}/Environments/{runEnvironment}-Spin.resource.json";

        return new ConfigurationBuilder()
            .AddInMemoryCollection(appConfig)
            .AddJsonFile(configFile)
            .AddPropertyResolver()
            .Build()
            .Bind<DirectoryOption>();
    }

    public static async Task<T> Run<T>(this DirectoryOption directoryOption, Func<DirectoryClient, Task<T>> action, ILoggerFactory? loggerFactory = null)
        => await Run<T>(directoryOption.HostUrl, directoryOption.ApiKey, action, loggerFactory);

    public static async Task<T> Run<T>(string hostUrl, string apiKey, Func<DirectoryClient, Task<T>> action, ILoggerFactory? loggerFactory = null)
    {
        hostUrl.VerifyNotEmpty(nameof(hostUrl));
        apiKey.VerifyNotEmpty(nameof(apiKey));
        action.VerifyNotNull(nameof(action));

        ILoggerFactory factory = loggerFactory ?? LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        ILogger<DirectoryClient> logger = factory.CreateLogger<DirectoryClient>();

        try
        {
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(hostUrl);
            httpClient.DefaultRequestHeaders.Add(Constants.ApiKeyName, apiKey);

            var client = new DirectoryClient(httpClient, factory.CreateLogger<DirectoryClient>());
            return await action(client);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Cannot connect to directory {hostUrl}", hostUrl);
            throw;
        }
    }
}
