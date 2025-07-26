//using Microsoft.Extensions.DependencyInjection;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph;

//public interface IGraphMapFactory
//{
//    GraphMap Create();
//    GraphMap Create(IEnumerable<GraphNode> nodes, IEnumerable<GraphEdge> edges);
//    GraphMap Create(DataETag dataETag);
//    GraphMap Create(string json);
//}

//public class GraphMapFactory : IGraphMapFactory
//{
//    private readonly IServiceProvider? _serviceProvider;

//    public GraphMapFactory() { }
//    public GraphMapFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider.NotNull();

//    public GraphMap Create() => _serviceProvider switch
//    {
//        null => new GraphMap(),
//        _ => new GraphMap(_serviceProvider.GetRequiredService<GraphMapCounter>()),
//    };

//    public GraphMap Create(IEnumerable<GraphNode> nodes, IEnumerable<GraphEdge> edges) => _serviceProvider switch
//    {
//        null => new GraphMap(nodes, edges),
//        _ => new GraphMap(nodes, edges, _serviceProvider.GetRequiredService<GraphMapCounter>()),
//    };

//    public GraphMap Create(DataETag dataETag) => _serviceProvider switch
//    {
//        null => dataETag.ToObject<GraphSerialization>().FromSerialization(),
//        _ => dataETag.ToObject<GraphSerialization>().FromSerialization(_serviceProvider.GetRequiredService<GraphMapCounter>()),
//    };

//    public GraphMap Create(string json) => _serviceProvider switch
//    {
//        null => json.NotEmpty().ToObject<GraphSerialization>().NotNull().FromSerialization(),
//        _ => json.NotEmpty().ToObject<GraphSerialization>().NotNull().FromSerialization(_serviceProvider.GetRequiredService<GraphMapCounter>()),
//    };
//}