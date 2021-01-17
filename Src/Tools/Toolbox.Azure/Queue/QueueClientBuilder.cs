using Microsoft.Extensions.Logging;
using System;
using Toolbox.Extensions;
using Toolbox.Services;
using Toolbox.Tools;

namespace Toolbox.Azure.Queue
{
    public class QueueClientBuilder<T> where T : class
    {
        public Func<T, Guid?>? GetId { get; set; }

        public IAwaiterCollection<T>? AwaiterCollection { get; set; }

        public ILoggerFactory? LoggerFactory { get; set; }

        public QueueOption? QueueOption { get; set; }

        public QueueClientBuilder<T> SetGetId(Func<T, Guid?> subject) => this.Action(x => x.GetId = subject);

        public QueueClientBuilder<T> SetAwaiterCollection(IAwaiterCollection<T> subject) => this.Action(x => x.AwaiterCollection = subject);

        public QueueClientBuilder<T> SetLoggerFactory(ILoggerFactory subject) => this.Action(x => x.LoggerFactory = subject);

        public QueueClientBuilder<T> SetQueueOption(QueueOption subject) => this.Action(x => x.QueueOption = subject);

        public QueueClient<T> Build()
        {
            GetId.VerifyNotNull($"{nameof(GetId)} is required");
            LoggerFactory.VerifyNotNull($"{nameof(LoggerFactory)} is required");
            QueueOption.VerifyNotNull($"{nameof(QueueOption)} is required");

            AwaiterCollection ??= new AwaiterCollection<T>();

            return new QueueClient<T>(GetId, QueueOption, AwaiterCollection, LoggerFactory.CreateLogger<QueueClient<T>>());
        }
    }
}