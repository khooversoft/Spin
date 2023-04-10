//using System.Threading.Tasks;
//using Directory.sdk;
//using Toolbox.Broker;

//namespace MessageNet.sdk.Host
//{
//    public interface IMessageHost
//    {
//        Client Client { get; }
//        IDirectoryNameService Directory { get; }
//        Receiver Receiver { get; }
//        Router Router { get; }

//        ValueTask DisposeAsync();
//        void Start(string serviceId);
//        Task Stop();
//    }
//}