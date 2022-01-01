using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Tools;

namespace Directory.sdk.Client
{
    public static class DirectoryHost
    {
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

            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(hostUrl);
            httpClient.DefaultRequestHeaders.Add(Constants.ApiKeyName, apiKey);

            var client = new DirectoryClient(httpClient, factory.CreateLogger<DirectoryClient>());
            return await action(client);
        }
    }
}
