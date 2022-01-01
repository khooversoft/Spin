//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Threading.Tasks;
//using Directory.sdk;
//using Directory.sdk.Model;
//using MessageNet.sdk.Protocol;
//using Microsoft.Extensions.Logging;
//using Toolbox.Application;
//using Toolbox.Azure.Queue;
//using Toolbox.Extensions;
//using Toolbox.Services;
//using Toolbox.Tools;

//namespace MessageNet.sdk.Host
//{
//    public class Client
//    {
//        private readonly ConcurrentDictionary<string, QueueClient<Message>> _clients = new ConcurrentDictionary<string, QueueClient<Message>>(StringComparer.OrdinalIgnoreCase);
//        private readonly IDirectoryNameService _directoryNameService;
//        private readonly AwaiterCollection<Message> _awaiterCollection;
//        private readonly ILoggerFactory _loggerFactory;
//        private string? _fromServiceId;

//        internal Client(IDirectoryNameService directoryNameService, AwaiterCollection<Message> awaiterCollection, ILoggerFactory loggerFactory)
//        {
//            directoryNameService.VerifyNotNull(nameof(directoryNameService));
//            awaiterCollection.VerifyNotNull(nameof(awaiterCollection));
//            loggerFactory.VerifyNotNull(nameof(loggerFactory));

//            _directoryNameService = directoryNameService;
//            _awaiterCollection = awaiterCollection;
//            _loggerFactory = loggerFactory;
//        }

//        public void SetFromId(string serviceId) => this.Action(x => x._fromServiceId = serviceId);

//        public QueueClient<Message> GetClient(string serviceId)
//        {
//            var db = _directoryNameService.Default;

//            QueueRecord queueRecord = db
//                .Service.Get(serviceId)
//                .Func(x => db.Queue.Get(x.Channel));

//            return _clients.GetOrAdd(serviceId, key => new ClientBuilder()
//            {
//                LoggerFactory = _loggerFactory,
//                QueueOption = queueRecord.ConvertTo(),
//            }.Build()
//            );
//        }

//        public async Task Send(Message message)
//        {
//            message.Verify();
//            string serviceId = message.Url.VerifyNotNull($"{nameof(Message.Url)} is required").Service;

//            message = UpdateFromIdIfRequired(message);
//            message = AddHeaders(message);

//            await GetClient(serviceId)
//                .Send(message);
//        }

//        public async Task<Message> Call(Message message)
//        {
//            message = UpdateFromIdIfRequired(message);
//            message = AddHeaders(message);
//            message.Verify();

//            await GetClient(message.Url.Service)
//                .Send(message);

//            var tcs = new TaskCompletionSource<Message>();
//            _awaiterCollection.Register(message.MessageId, tcs);

//            return await tcs.Task;
//        }

//        public async Task<T> Get<T>(MessageUrl url, object? content = null)
//        {
//            var message = new MessageBuilder()
//                .SetMethod(MessageMethod.get)
//                .SetUrl(url)
//                .AddContent(content?.ToContent())
//                .Build();

//            Message result = (await Call(message)).Verify();
//            if (typeof(T) == typeof(Message)) return (T)(object)result;

//            result.EnsureSuccessStatusCode();

//            return result
//                .Verify()
//                .Contents[0]
//                .ConvertTo<T>();
//        }

//        public async Task<bool> Delete(MessageUrl url, object? content = null)
//        {
//            var message = new MessageBuilder()
//                .SetMethod(MessageMethod.get)
//                .SetUrl(url)
//                .AddContent(content?.ToContent())
//                .Build();

//            Message result = (await Call(message)).Verify();

//            if (result.Status == HttpStatusCode.NotFound) return false;
//            result.EnsureSuccessStatusCode();

//            return true;
//        }

//        public async Task<bool> Post(MessageUrl url, object content)
//        {
//            Message result = await Post<Message>(url, content);

//            if (result.Status == HttpStatusCode.NotFound) return false;
//            result.EnsureSuccessStatusCode();

//            return true;
//        }

//        public async Task<T> Post<T>(MessageUrl url, object content)
//        {
//            var message = new MessageBuilder()
//                .SetMethod(MessageMethod.post)
//                .SetUrl(url)
//                .AddContent(content.ToContent())
//                .Build();

//            Message result = (await Call(message)).Verify();
//            if (typeof(T) == typeof(Message)) return (T)(object)result;

//            result.EnsureSuccessStatusCode();

//            return result
//                .Verify()
//                .Contents[0]
//                .ConvertTo<T>();
//        }

//        private Message UpdateFromIdIfRequired(Message message)
//        {
//            if (message.From != null) return message;

//            _fromServiceId.VerifyNotNull($"From endpoint is required and message's from endpoint is null");

//            return message
//                .Verify()
//                .ToBuilder()
//                .SetFrom(new MessageUrl(_fromServiceId))
//                .Build();
//        }

//        private Message AddHeaders(Message message)
//        {
//            message.Verify();

//            ServiceRecord service = _directoryNameService.Default.Service.Get(message.Url.Service);

//            return message
//                .Verify()
//                .ToBuilder()
//                .AddHeader(new Header { Name = Constants.ApiKeyName, Value = service.ApiKey })
//                .Build();
//        }
//    }
//}