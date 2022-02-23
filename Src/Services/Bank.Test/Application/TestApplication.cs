using Bank.sdk.Client;
using BankApi.Application;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spin.Common.Client;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using Toolbox.Application;

namespace Bank.Test.Application;

internal enum BankName
{
    First,
    Second,
}

internal static class TestApplication
{
    private static ApiHost?[] _hosts = new ApiHost?[2];
    private static object _lock = new object();

    public static ApiHost GetHost(BankName bank)
    {
        lock (_lock)
        {
            string hostName = bank switch
            {
                BankName.First => "Bank-First",
                BankName.Second => "Bank-Second",

                _ => throw new ArgumentException($"Unknown bank={bank}")
            };

            return _hosts[(int)bank] ??= new ApiHost(hostName);
        }
    }
}


internal class ApiHost
{
    private HttpClient? _client;
    private WebApplicationFactory<Program> _host = null!;
    private readonly string _bankName;
    private object _lock = new object();

    public ApiHost(string bankName)
    {
        _bankName = bankName;
    }

    public HttpClient GetClient()
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

                    builder.ConfigureAppConfiguration(app =>
                    {
                        app.AddCommandLine(new[] { $"BankName={_bankName}" });
                    });
                });

            ApplicationOption option = _host.Services.GetRequiredService<ApplicationOption>();

            _client = _host.CreateClient();
            _client.DefaultRequestHeaders.Add(Constants.ApiKeyName, option.ApiKey);
            _client.DefaultRequestHeaders.Add(Constants.BypassCacheName, "true");

            return _client;
        }
    }

    public PingClient GetPingClient() => new PingClient(GetClient(), _host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<PingClient>());

    public BankAccountClient GetBankAccountClient() => new BankAccountClient(GetClient(), _host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<BankAccountClient>());

    public BankTransactionClient GetBankTransactionClient() => new BankTransactionClient(GetClient(), _host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<BankTransactionClient>());


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

               Debugger.Break();
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
