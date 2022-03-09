using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Logging;
using Toolbox.Services;
using Toolbox.Tools;

namespace Toolbox.Azure.Queue.RPC;

public class QueueRpcClient<T> : QueueClient<T>, IAsyncDisposable where T : class
{
    private readonly Func<T, Guid?> _getId;
    private readonly IAwaiterCollection<T> _awaiterService;
    private readonly ILogger<QueueClient<T>> _logger;

    public QueueRpcClient(Func<T, Guid?> getId, QueueOption queueOption, IAwaiterCollection<T> awaiterService, ILogger<QueueClient<T>> logger)
        : base(queueOption, logger)
    {
        getId.VerifyNotNull(nameof(getId));
        awaiterService.VerifyNotNull(nameof(awaiterService));

        _getId = getId;
        _awaiterService = awaiterService;
        _logger = logger;
    }

    /// <summary>
    /// Call endpoint, expect a single return message
    /// </summary>
    /// <param name="payload">payload for message</param>
    /// <param name="timeout">timeout, null for default</param>
    /// <returns></returns>
    public async Task<T> Call(T payload, TimeSpan? timeout = null)
    {
        Message message = payload.ToMessage(Guid.NewGuid().ToString());

        _logger.Trace($"Calling message: contentType={message.ContentType}, data.Length {message.Body?.Length}");

        await _messageSender!.SendAsync(message);

        return await WaitForResponse(_getId(payload), timeout);
    }

    private Task<T> WaitForResponse(Guid? messageId, TimeSpan? timeout = null)
    {
        if (messageId == null) return Task.FromResult<T>(default!);

        _logger.Trace($"Registering for message receive event.  messageId={messageId}");

        var tcs = new TaskCompletionSource<T>();
        _awaiterService.Register((Guid)messageId, tcs, timeout);

        return tcs.Task;
    }
}
