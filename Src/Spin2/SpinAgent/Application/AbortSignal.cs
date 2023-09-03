using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinAgent.Application;

internal class AbortSignal
{
    private readonly ILogger<AbortSignal> _logger;
    private CancellationTokenSource? _tokenSource;

    public AbortSignal(ILogger<AbortSignal> logger)
    {
        _logger = logger.NotNull();
    }

    public CancellationToken GetToken() => _tokenSource?.Token ?? throw new InvalidOperationException("Not started");

    public void StartTracking()
    {
        var context = new ScopeContext(_logger);
        context.Location().LogInformation("Starting to track abort signals");

        _tokenSource = new CancellationTokenSource();
        Console.CancelKeyPress += Console_CancelKeyPress;
    }

    public void StopTracking()
    {
        var context = new ScopeContext(_logger);
        context.Location().LogInformation("Stoping tracking of abort signals");

        Console.CancelKeyPress -= Console_CancelKeyPress;
        _tokenSource = null;
    }

    private void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        const string msg = "Contract is shutting down...";

        _logger.LogInformation(msg);
        (_tokenSource ?? throw new UnreachableException(msg)).Cancel();
    }
}
