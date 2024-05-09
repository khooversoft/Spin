using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class GraphClientTool
{
    public static async Task<Option<GraphQueryResult>> SetEntity<T>(this IGraphClient graphClient, T subject, ScopeContext context) where T : class
    {
        graphClient.NotNull();
        subject.NotNull();

        var commandsOption = subject.GetGraphCommands();
        if (commandsOption.IsError()) return commandsOption.ToOptionStatus<GraphQueryResult>();

        var commands = commandsOption.Return();
        var entityNodeCommand = commands.GetEntityNodeCommand();
        if (entityNodeCommand.IsError()) return entityNodeCommand.ToOptionStatus<GraphQueryResult>();

        string cmds = commands.Select(x => x.GetAddCommand()).Join(Environment.NewLine);

        var result = await graphClient.ExecuteScalar(cmds, context);
        return result;
    }
}
