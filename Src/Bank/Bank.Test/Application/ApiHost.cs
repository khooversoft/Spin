using Bank.sdk.Client;
using Bank.sdk.Service;
using BankApi.Application;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spin.Common.Client;
using System.Net.Http;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Azure.Queue;

namespace Bank.Test.Application;

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

    public PingClient GetPingClient() =>
        new PingClient(GetClient(), _host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<PingClient>());

    public BankAccountClient GetBankAccountClient() =>
        new BankAccountClient(GetClient(), _host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<BankAccountClient>());

    public BankTransactionClient GetBankTransactionClient() =>
        new BankTransactionClient(GetClient(), _host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<BankTransactionClient>());

    public BankClearingClient GetBankClearingClient() =>
        new BankClearingClient(GetClient(), _host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<BankClearingClient>());

    public async Task ResetQueue()
    {
        GetClient();

        BankHost host = _host.Services.GetRequiredService<BankHost>();
        QueueOption queueOption = await host.BankDirectory.GetQueueOption();

        QueueAdmin admin = new QueueAdmin(queueOption, _host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<QueueAdmin >());
        await admin.DeleteIfExist(queueOption.QueueName);

        var definition = new QueueDefinition
        {
            QueueName = queueOption.QueueName,
        };

        await admin.CreateIfNotExist(definition);

        var getDefinition = await admin.GetDefinition(queueOption.QueueName);
        getDefinition.Should().NotBeNull();
        getDefinition.QueueName.Should().Be(queueOption.QueueName);
    }
}
