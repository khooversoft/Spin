//using Microsoft.Extensions.DependencyInjection;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Data;

//public class DataPipelineHandlerCollection
//{
//    private readonly IList<Func<IServiceProvider, IDataProvider>> _handlers = new List<Func<IServiceProvider, IDataProvider>>();

//    public void Add(Func<IServiceProvider, IDataProvider> handler) => _handlers.Add(handler.NotNull());
//    public void Add<T>() where T : class, IDataProvider => _handlers.Add(service => service.GetRequiredService<T>());

//    internal Option<IDataProvider> BuildHandlers(IServiceProvider serviceProvider)
//    {
//        var result = _handlers
//            .Reverse()
//            .Aggregate((IDataProvider?)null, (prev, current) =>
//            {
//                var handler = current(serviceProvider);
//                handler.InnerHandler = prev;
//                return handler;
//            });

//        return result switch
//        {
//            null => StatusCode.NotFound,
//            _ => result.ToOption<IDataProvider>(),
//        };
//    }
//}
