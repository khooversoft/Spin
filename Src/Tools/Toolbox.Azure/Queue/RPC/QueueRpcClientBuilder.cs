using Microsoft.Extensions.Logging;
using System;
using Toolbox.Extensions;
using Toolbox.Services;
using Toolbox.Tools;

namespace Toolbox.Azure.Queue.RPC;

public class QueueRpcClientBuilder<T> where T : class
{
    public Func<T, Guid?>? GetId { get; set; } = x => null;

    public IAwaiterCollection<T>? AwaiterCollection { get; set; }

    public ILoggerFactory? LoggerFactory { get; set; }

    public QueueOption? QueueOption { get; set; }

    public QueueRpcClientBuilder<T> SetGetId(Func<T, Guid?> func) => this.Action(x => x.GetId = func);
    public QueueRpcClientBuilder<T> SetAwaiterCollection(IAwaiterCollection<T> awaiterCollection) => this.Action(x => x.AwaiterCollection = awaiterCollection);
    public QueueRpcClientBuilder<T> SetLoggerFactory(ILoggerFactory loggerFactory) => this.Action(x => x.LoggerFactory = loggerFactory);
    public QueueRpcClientBuilder<T> SetQueueOption(QueueOption queueOption) => this.Action(x => x.QueueOption = queueOption);

    public QueueClient<T> Build()
    {
        GetId.VerifyNotNull($"{nameof(GetId)} is required");
        LoggerFactory.VerifyNotNull($"{nameof(LoggerFactory)} is required");
        QueueOption.VerifyNotNull($"{nameof(QueueOption)} is required");

        AwaiterCollection ??= new AwaiterCollection<T>(LoggerFactory.CreateLogger<AwaiterCollection<T>>());

        return new QueueRpcClient<T>(GetId, QueueOption, AwaiterCollection, LoggerFactory.CreateLogger<QueueClient<T>>());
    }
}
