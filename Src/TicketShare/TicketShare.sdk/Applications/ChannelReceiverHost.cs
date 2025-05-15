using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public abstract class ChannelReceiverHost<T> : IHostedService
{
    private readonly Channel<T> _channel;
    private readonly ILogger _logger;
    private readonly ScopeContext _context;
    private readonly string _name;
    private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
    private Task? _runningTask;

    public ChannelReceiverHost(string name, Channel<T> channel, ILogger logger)
    {
        _name = name.NotEmpty();
        _channel = channel.NotNull();
        _logger = logger.NotNull();
        _context = _logger.ToScopeContext();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _runningTask = Run(_tokenSource.Token);
        _context.LogDebug("Starting {name} background service", _name);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _tokenSource.Cancel();
        _context.LogDebug("Stopping {name} background service", _name);

        var task = Interlocked.Exchange(ref _runningTask, null);
        if (task != null)
        {
            _context.LogDebug("Waiting on {name} background service to stop", _name);
            await task;
            _context.LogDebug("Back from processing on {name} background service to stop", _name);
        }

        _context.LogDebug("Stopped {name} background service", _name);
    }

    private Task Run(CancellationToken token)
    {
        var tcs = new TaskCompletionSource();
        var context = new ScopeContext(_logger);
        context.LogDebug("Starting {service} background service", _name);

        Task.Run(async () =>
        {
            try
            {
                while (!token.IsCancellationRequested && await _channel.Reader.WaitToReadAsync(_tokenSource.Token))
                {
                    if (!_channel.Reader.TryRead(out var channelMessage)) continue;

                    var result = await ProcessMessage(channelMessage, context).ConfigureAwait(false);

                    if (result.IsError())
                    {
                        context.LogError("Failed to process channel={name}, message{message}, error={error}", _name, channelMessage, result.Error);
                        continue;
                    }

                    context.LogInformation("Processed channel={name}, message{message}", _name, channelMessage);
                }
            }
            catch (OperationCanceledException)
            {
                context.LogDebug("Operation cancelled on {service} background service", _name);
            }
            catch (Exception ex)
            {
                context.LogError(ex, "Failed to process channel={name}", _name);
            }

            context.LogInformation("Exiting {service} background service", _name);
            tcs.SetResult();
        });

        return tcs.Task;
    }

    protected abstract Task<Option> ProcessMessage(T message, ScopeContext context);
}
