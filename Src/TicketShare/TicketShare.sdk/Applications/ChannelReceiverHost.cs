using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public abstract class ChannelReceiverHost<T> : BackgroundService
{
    private readonly Channel<T> _channel;
    private readonly ILogger _logger;
    private readonly string _name;

    public ChannelReceiverHost(string name, Channel<T> channel, ILogger logger)
    {
        _name = name.NotEmpty();
        _channel = channel.NotNull();
        _logger = logger.NotNull();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tcs = new TaskCompletionSource();
        var context = new ScopeContext(_logger);
        context.LogInformation("Starting {service} background service", _name);

        Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested && await _channel.Reader.WaitToReadAsync(stoppingToken))
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

            context.LogInformation("Exiting {service} background service", _name);
            tcs.SetResult();
        });

        return tcs.Task;
    }

    protected abstract Task<Option> ProcessMessage(T message, ScopeContext context);
}
