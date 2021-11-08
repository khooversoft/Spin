using Directory.sdk;
using MessageNet.sdk.Protocol;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Toolbox.Broker;
using Toolbox.Services;
using Toolbox.Tools;

namespace MessageNet.sdk.Host
{
    public class MessageHost : IMessageHost, IAsyncDisposable
    {
        public MessageHost(IDirectoryNameService directoryNameService, ILoggerFactory loggerFactory)
        {
            directoryNameService.VerifyNotNull(nameof(directoryNameService));
            loggerFactory.VerifyNotNull(nameof(loggerFactory));

            var awaiterCollection = new AwaiterCollection<Message>(loggerFactory.CreateLogger<AwaiterCollection<Message>>());

            Directory = directoryNameService;
            Client = new Client(Directory, awaiterCollection, loggerFactory);
            Router = new Router(loggerFactory.CreateLogger<Router>());

            Receiver = new ReceiverBuilder()
            {
                AwaiterCollection = awaiterCollection,
                Directory = Directory,
                LoggerFactory = loggerFactory,
            }.Build();
        }

        public IDirectoryNameService Directory { get; }

        public Client Client { get; }

        public Receiver Receiver { get; }

        public Router Router { get; }

        public async ValueTask DisposeAsync() => await Stop();

        public void Start(string serviceId)
        {
            serviceId.VerifyNotEmpty(nameof(serviceId), message: "Service ID is required to start listener");

            Client.SetFromId(serviceId);

            // Start receiver
            Receiver.Start(serviceId, async message =>
            {
                string path = new StringVector() + message.Url.Endpoint + message.Method;
                await Router.Send(path, message);
            });
        }

        public async Task Stop()
        {
            await Receiver.StopAll();
        }
    }
}