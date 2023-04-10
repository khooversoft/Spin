using Directory.sdk.Client;
using DirectoryApi.Application;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spin.Common.Client;
using System.Linq;
using System.Net.Http;
using Toolbox.Application;

namespace Directory.Test.Application;

internal static class TestApplication
{
    private static HttpClient? _client;
    private static WebApplicationFactory<Program> _host = null!;
    private static object _lock = new object();

    public static HttpClient GetClient()
    {
        lock (_lock)
        {
            if (_client != null) return _client;

            ILogger logger = LoggerFactory.Create(builder =>
            {
                builder.AddDebug();
            }).CreateLogger<Program>();

            _host = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Test");
                    builder.ConfigureServices(service => ConfigureModelBindingExceptionHandling(service, logger));
                });

            ApplicationOption option = _host.Services.GetRequiredService<ApplicationOption>();

            _client = _host.CreateClient();
            _client.DefaultRequestHeaders.Add(Constants.ApiKeyName, option.ApiKey);
            _client.DefaultRequestHeaders.Add(Constants.BypassCacheName, "true");

            return _client;
        }
    }

    public static PingClient GetPingClient() => new PingClient(GetClient(), _host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<PingClient>());

    public static DirectoryClient GetDirectoryClient() => new DirectoryClient(GetClient(), _host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<DirectoryClient>());
    
    public static IdentityClient GetIdentityClient() => new IdentityClient(GetClient(), _host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<IdentityClient>());

    public static SigningClient GetSigningClient() => new SigningClient(GetClient(), _host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<SigningClient>());

    private static void ConfigureModelBindingExceptionHandling(IServiceCollection services, ILogger logger)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = actionContext =>
            {
                ValidationProblemDetails? error = actionContext.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .Select(e => new ValidationProblemDetails(actionContext.ModelState))
                    .FirstOrDefault();

                logger.LogError("ApiBehaviorOption error");

                // Here you can add logging to you log file or to your Application Insights.
                // For example, using Serilog:
                // Log.Error($"{{@RequestPath}} received invalid message format: {{@Exception}}", 
                //   actionContext.HttpContext.Request.Path.Value, 
                //   error.Errors.Values);
                return new BadRequestObjectResult(error);
            };
        });
    }
}
