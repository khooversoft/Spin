//using Toolbox.Data;
//using Toolbox.Types;


//namespace Toolbox.Test.Application;

//public static class DataClientTool
//{
//    public static IEnumerable<IDataProvider> GetDataProviders<T>(this IDataClient<T> dataClient)
//    {
//        IDataProvider handler = dataClient switch
//        {
//            DataClient<T> client => client.Handler,
//            _ => throw new InvalidOperationException("Unsupported data client type")
//        };

//        var list = new Sequence<IDataProvider>();
//        list += handler;

//        while (handler.InnerHandler is not null)
//        {
//            handler = handler.InnerHandler;
//            list += handler;
//        }

//        return list;
//    }
//}
