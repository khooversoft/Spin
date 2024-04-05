using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphEntityAccess
{
    private readonly GraphDbContext _graphDbContext;
    internal GraphEntityAccess(GraphDbContext graphDbContext) => _graphDbContext = graphDbContext.NotNull();

    public async Task<Option<string>> Set<T>(T subject, ScopeContext context) where T : class
    {
        IReadOnlyList<IGraphEntityCommand> entityNodeCommands = subject.GetGraphCommands().ThrowOnError().Return();
        NodeCreateCommand entityNodeCommand = entityNodeCommands.GetEntityNodeCommand();

        GraphQueryResult entityNodeResult = (await _graphDbContext.Graph.ExecuteScalar(entityNodeCommand.GetSearchCommand(), context)).ThrowOnError().Return();
        GraphNode? entityNode = entityNodeResult.Status.IsOk() ? entityNodeResult.Items.OfType<GraphNode>().First() : null;

        bool storeDocumentExist = (await _graphDbContext.Store.Exist(entityNodeCommand.NodeKey, GraphConstants.EntityName, context)).IsOk();

        switch ((entityNode, storeDocumentExist))
        {
            case (GraphNode, true): break;
            case (GraphNode, false): return (StatusCode.NotFound, $"Missing entity file for node: {entityNodeCommand.NodeKey}");
            case (null, true): return (StatusCode.Conflict, $"Missing entity node for file: {entityNodeCommand.NodeKey}");
        }

        Option result = entityNode switch
        {
            null => await Add(entityNodeCommands, subject, context),
            //_ => await Update(entityNodeCommands, subject, context),
        };

        return StatusCode.OK;
    }

    public async Task<Option> Remove(string entryFileId, ScopeContext context)
    {
        return await _graphDbContext.Store.Delete(entryFileId, GraphConstants.EntityName, context);
    }

    private async Task<Option> Add<T>(IReadOnlyList<IGraphEntityCommand> commands, T subject, ScopeContext context) where T : class
    {
        string cmd = commands.Select(x => x.GetAddCommand()).Join(" ");
        GraphQueryResult entityNodeResult = (await _graphDbContext.Graph.ExecuteScalar(cmd, context)).ThrowOnError().Return();
        if (entityNodeResult.Status.IsError()) return entityNodeResult.Status;

        var nodeKey = commands.GetEntityNodeCommand().NodeKey;
        var result = await _graphDbContext.Store.Add<T>(nodeKey, GraphConstants.EntityName, subject, context);
        return result.ToOptionStatus();
    }

    //private async Task<Option> Update<T>(IReadOnlyList<IGraphEntityCommand> commands, T subject, ScopeContext context)
    //{
    //    string nodeKey = commands.OfType<NodeCreateCommand>().Where(x => x.IsEntityNode).First().NodeKey;
    //}
}
