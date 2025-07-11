//using System.Diagnostics;
//using Microsoft.Extensions.Logging;
//using Toolbox.Types;

//namespace Toolbox.Tools;

//public class ConsoleAbortSignal : IDisposable
//{
//    private readonly ILogger _logger;
//    private CancellationTokenSource _tokenSource = new CancellationTokenSource();
//    private bool _initialized;

//    public ConsoleAbortSignal(ILogger logger) => _logger = logger.NotNull();

//    public void Dispose() => StopTracking(new ScopeContext(_logger));

//    public CancellationToken GetToken() => _tokenSource.Token;

//    public IDisposable StartTracking(ScopeContext context)
//    {
//        bool initialized = Interlocked.CompareExchange(ref _initialized, true, false);
//        if (initialized) return this;

//        context.Location().LogDebug("Starting to track abort signals");
//        Console.CancelKeyPress += Console_CancelKeyPress;

//        return new StopTrackingDisposable(this);
//    }

//    public void StopTracking(ScopeContext context)
//    {
//        _initialized = Interlocked.Exchange(ref _initialized, false);

//        context.Location().LogDebug("Stopping tracking of abort signals");
//        Console.CancelKeyPress -= Console_CancelKeyPress;
//    }

//    private void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
//    {
//        e.Cancel = true;
//        const string msg = "Shutting down...";

//        _logger.LogDebug(msg);
//        _tokenSource.Cancel();
//    }

//    private readonly struct StopTrackingDisposable : IDisposable
//    {
//        private readonly ConsoleAbortSignal _signal;
//        public StopTrackingDisposable(ConsoleAbortSignal signal) => _signal = signal;

//        public void Dispose() => _signal.StopTracking(new ScopeContext(_signal._logger));
//    }
//}
