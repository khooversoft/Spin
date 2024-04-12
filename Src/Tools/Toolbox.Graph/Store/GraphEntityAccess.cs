using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphEntityAccess
{
    private readonly GraphDbAccess _graphDbContext;
    internal GraphEntityAccess(GraphDbAccess graphDbContext) => _graphDbContext = graphDbContext.NotNull();

    public async Task<Option<string>> Set<T>(T subject, ScopeContext context) where T : class
    {
        IReadOnlyList<IGraphEntityCommand> commands = subject.GetGraphCommands().ThrowOnError().Return();
        var entityNodeCommand = commands.GetEntityNodeCommand();
        if (entityNodeCommand.IsError()) return entityNodeCommand.ToOptionStatus<string>();

        string cmds = commands.Select(x => x.GetAddCommand()).Join(Environment.NewLine);
        GraphQueryResult cmdResult = (await _graphDbContext.Graph.ExecuteScalar(cmds, context)).ThrowOnError().Return();
        if (cmdResult.Status.IsError()) return cmdResult.Status.ToOptionStatus<string>();

        var nodeKey = commands.GetEntityNodeCommand().Return().NodeKey.NotEmpty();
        var setStatus = await _graphDbContext.Store.Set<T>(nodeKey, GraphConstants.EntityName, subject, context);

        return setStatus;
    }

    public async Task<Option> Delete<T>(T subject, ScopeContext context) where T : class
    {
        NodeCreateCommand entityNodeCommand = subject
            .GetGraphCommands().ThrowOnError().Return()
            .GetEntityNodeCommand().ThrowOnError().Return();

        string command = entityNodeCommand.GetDeleteCommand();
        var result = await _graphDbContext.Graph.ExecuteScalar(command, context);

        return result.ToOptionStatus();
    }

}
