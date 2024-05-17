namespace Toolbox.Graph;

public static class GraphClientTool
{
    //public static async Task<Option<GraphQueryResult>> SetEntity<T>(this IGraphClient graphClient, T subject, ScopeContext context) where T : class
    //{
    //    graphClient.NotNull();
    //    subject.NotNull();

    //    var commandsOption = subject.GetGraphCommands();
    //    if (commandsOption.IsError()) return commandsOption.ToOptionStatus<GraphQueryResult>();

    //    var commands = commandsOption.Return();
    //    var entityNodeCommand = commands.GetEntityNodeCommands();
    //    if (entityNodeCommand.IsError()) return entityNodeCommand.ToOptionStatus<GraphQueryResult>();

    //    string cmds = commands.Select(x => x.GetAddCommand()).Join(Environment.NewLine);

    //    var result = await graphClient.Execute(cmds, context);
    //    return result;
    //}
}
