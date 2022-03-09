using Bank.sdk.Model;
using Bank.sdk.Service;
using System.Collections.Concurrent;
using Toolbox.Azure.Queue;
using Toolbox.Logging;
using Toolbox.Tools;

namespace BankApi.Service;

public class ClearingHostedService : IHostedService
{
    private readonly BankClearingService _bankClearingService;
    private readonly ILogger<ClearingHostedService> _logger;

    public ClearingHostedService(BankClearingService bankClearingService, ILogger<ClearingHostedService> logger)
    {
        _bankClearingService = bankClearingService;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken token)
    {
        _logger.Information("Starting clearing service");

        await _bankClearingService.Start(token);
        _logger.Information("Clearing service started");
    }

    public async Task StopAsync(CancellationToken token)
    {
        await _bankClearingService.Stop();
    }
}
