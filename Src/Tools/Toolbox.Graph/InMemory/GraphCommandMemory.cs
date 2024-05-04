//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph;

//public class GraphCommandMemory : IGraphCommand
//{
//    private readonly GraphMemoryContext _graphDbContext;
//    internal GraphCommandMemory(GraphMemoryContext graphDbContext) => _graphDbContext = graphDbContext.NotNull();

//    public async Task<Option<GraphQueryResults>> Execute(string graphQuery, ScopeContext context)
//    {
//        var graphContext = new GraphContext(_graphDbContext.Map, _graphDbContext.FileStore, _graphDbContext.ChangeTrace, context);
//        var result = await GraphCommand.Execute(graphContext, graphQuery);
//        return result;
//    }

//    public async Task<Option<GraphQueryResult>> ExecuteScalar(string graphQuery, ScopeContext context)
//    {
//        var graphContext = new GraphContext(_graphDbContext.Map, _graphDbContext.FileStore, _graphDbContext.ChangeTrace, context);
//        var result = await GraphCommand.Execute(graphContext, graphQuery);
//        if (result.IsError()) return result.ToOptionStatus<GraphQueryResult>();

//        return result.Return().Items[0];
//    }
//}

//public class GraphCommandClient : IGraphCommand
//{
//    private readonly IGraphContext _graphContext;
//    public GraphCommandClient(IGraphContext graphContext) => _graphContext = graphContext.NotNull();

//    public async Task<Option<GraphQueryResults>> Execute(string graphQuery, ScopeContext context)
//    {
//        var result = await GraphCommand.Execute(_graphContext, graphQuery);
//        return result;
//    }

//    public async Task<Option<GraphQueryResult>> ExecuteScalar(string graphQuery, ScopeContext context)
//    {
//        var result = await GraphCommand.Execute(_graphContext, graphQuery);
//        if (result.IsError()) return result.ToOptionStatus<GraphQueryResult>();

//        return result.Return().Items[0];
//    }
//}