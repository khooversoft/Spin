//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Toolbox.Tools;

//namespace Toolbox.Azure.Queue
//{
//    public class QueueReceiverFactory : IQueueReceiverFactory
//    {
//        private readonly ILoggerFactory _loggerFactory;

//        public QueueReceiverFactory(ILoggerFactory loggerFactory)
//        {
//            loggerFactory.VerifyNotNull(nameof(loggerFactory));

//            _loggerFactory = loggerFactory;
//        }

//        public IQueueReceiver Create<T>(QueueReceiverOption<T> queueReceiver) where T : class =>
//            new QueueReceiver<T>(queueReceiver, _loggerFactory.CreateLogger<QueueReceiver<T>>());
//    }
//}
