using Microsoft.Extensions.Logging;
using System;
using Toolbox.Extensions;
using Toolbox.Services;
using Toolbox.Tools;

namespace Toolbox.Azure.Queue
{
    public class QueueClientBuilder<T> where T : class
    {
        public Func<T, Guid?>? GetId { get; set; } = x => null;

        public IAwaiterCollection<T>? AwaiterCollection { get; set; }

        public ILoggerFactory? LoggerFactory { get; set; }

        public QueueOption? QueueOption { get; set; }

        public QueueClientBuilder<T> SetGetId(Func<T, Guid?> func) => this.Action(x => x.GetId = func);
        public QueueClientBuilder<T> SetAwaiterCollection(IAwaiterCollection<T> awaiterCollection) => this.Action(x => x.AwaiterCollection = awaiterCollection);
        public QueueClientBuilder<T> SetLoggerFactory(ILoggerFactory loggerFactory) => this.Action(x => x.LoggerFactory = loggerFactory);
        public QueueClientBuilder<T> SetQueueOption(QueueOption queueOption) => this.Action(x => x.QueueOption = queueOption);

        public QueueClient<T> Build()
        {
            GetId.VerifyNotNull($"{nameof(GetId)} is required");
            LoggerFactory.VerifyNotNull($"{nameof(LoggerFactory)} is required");
            QueueOption.VerifyNotNull($"{nameof(QueueOption)} is required");

            AwaiterCollection ??= new AwaiterCollection<T>(LoggerFactory.CreateLogger<AwaiterCollection<T>>());

            return new QueueClient<T>(GetId, QueueOption, AwaiterCollection, LoggerFactory.CreateLogger<QueueClient<T>>());
        }
    }
}