using MessageNet.sdk.Protocol;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Azure.Queue;
using Toolbox.Extensions;
using Toolbox.Services;
using Toolbox.Tools;

namespace MessageNet.sdk.Host
{
    public class MessageReceiverCollectionBuilder : MessageReceiverCollectionBuilder<MessagePacket>
    {
        public MessageReceiverCollectionBuilder()
        {
            GetId = x => x.GetOriginateMessageId();
        }
    }


    public class MessageReceiverCollectionBuilder<T> where T : class
    {
        public Func<T, Guid?>? GetId { get; set; }

        public IQueueReceiverFactory? QueueReceiverFactory { get; set; }

        public IAwaiterCollection<T>? AwaiterCollection { get; set; }

        public ILoggerFactory? LoggerFactory { get; set; }

        public MessageReceiverCollectionBuilder<T> SetGetId(Func<T, Guid?> subject) => this.Action(x => x.GetId = subject);

        public MessageReceiverCollectionBuilder<T> SetQueueReceiverFactory(IQueueReceiverFactory subject) => this.Action(x => x.QueueReceiverFactory = subject);

        public MessageReceiverCollectionBuilder<T> SetAwaiterCollection(IAwaiterCollection<T> subject) => this.Action(x => x.AwaiterCollection = subject);

        public MessageReceiverCollectionBuilder<T> SetLoggerFactory(ILoggerFactory subject) => this.Action(x => x.LoggerFactory = subject);


        public MessageReceiverCollection<T> Build()
        {
            GetId.VerifyNotNull($"{nameof(GetId)} is required");
            LoggerFactory.VerifyNotNull($"{nameof(LoggerFactory)} is required");

            AwaiterCollection ??= new AwaiterCollection<T>(LoggerFactory.CreateLogger<AwaiterCollection<T>>());
            QueueReceiverFactory ??= new QueueReceiverFactory(LoggerFactory);

            return new MessageReceiverCollection<T>(GetId, QueueReceiverFactory, AwaiterCollection, LoggerFactory.CreateLogger<MessageReceiverCollection<T>>());
        }
    }
}
