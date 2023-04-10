using Directory.sdk;
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
    public class ReceiverBuilder
    {
        public Func<Message, Guid?>? GetId { get; set; } = x => x.FromMessageId;

        public IQueueReceiverFactory? QueueReceiverFactory { get; set; }

        public IAwaiterCollection<Message>? AwaiterCollection { get; set; }

        public ILoggerFactory? LoggerFactory { get; set; }

        public IDirectoryNameService? Directory { get; set; }


        public Receiver Build()
        {
            GetId.VerifyNotNull($"{nameof(GetId)} is required");
            Directory.VerifyNotNull($"{nameof(Directory)} is required");
            LoggerFactory.VerifyNotNull($"{nameof(LoggerFactory)} is required");

            AwaiterCollection ??= new AwaiterCollection<Message>(LoggerFactory.CreateLogger<AwaiterCollection<Message>>());
            QueueReceiverFactory ??= new QueueReceiverFactory(LoggerFactory);

            return new Receiver(GetId, QueueReceiverFactory, AwaiterCollection, Directory, LoggerFactory.CreateLogger<Receiver>());
        }
    }
}
