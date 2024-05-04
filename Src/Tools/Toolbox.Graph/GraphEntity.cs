//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph;

//public class GraphEntity : IGraphEntity
//{
//    public GraphEntity(IGraphCommand command, IGraphStore store)
//    {
//        Command = command.NotNull();
//        Store = store.NotNull();
//    }

//    public IGraphCommand Command { get; }
//    public IGraphStore Store { get; }

//    public async Task<Option<string>> SetEntity<T>(T subject, ScopeContext context) where T : class
//    {
//        IReadOnlyList<IGraphEntityCommand> commands = subject.GetGraphCommands().ThrowOnError().Return();
//        var entityNodeCommand = commands.GetEntityNodeCommand();
//        if (entityNodeCommand.IsError()) return entityNodeCommand.ToOptionStatus<string>();

//        string cmds = commands.Select(x => x.GetAddCommand()).Join(Environment.NewLine);
//        GraphQueryResult cmdResult = (await Command.ExecuteScalar(cmds, context)).ThrowOnError().Return();
//        if (cmdResult.Status.IsError()) return cmdResult.Status.ToOptionStatus<string>();

//        var nodeKey = commands.GetEntityNodeCommand().Return().NodeKey.NotEmpty();
//        var setStatus = await Store.Set<T>(nodeKey, GraphConstants.EntityName, subject, context);

//        return setStatus;
//    }

//    public async Task<Option> DeleteEntity<T>(T subject, ScopeContext context) where T : class
//    {
//        NodeCreateCommand entityNodeCommand = subject
//            .GetGraphCommands().ThrowOnError().Return()
//            .GetEntityNodeCommand().ThrowOnError().Return();

//        string command = entityNodeCommand.GetDeleteCommand();
//        var result = await Command.ExecuteScalar(command, context);

//        return result.ToOptionStatus();
//    }
//}
