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

            _host = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment($"Test;BankName={_bankName}");
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

    public BankClearingClient GetBankClearingClient() => new BankClearingClient(GetClient(), _host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<BankClearingClient>());
}
