using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface ICommandBatchBuilder
{
    Option<string> BuildQuery(ScopeContext context);
}

public class CommandBatchBuilder
{
    private readonly Sequence<ICommandBatchBuilder> _commandBatchBuilders = new();

    public CommandBatchBuilder Add(ICommandBatchBuilder commandBatchBuilder)
    {
        _commandBatchBuilders.Add(commandBatchBuilder.NotNull());
        return this;
    }

    public CommandBatchBuilder Add(Func<ScopeContext, string> buildQuery)
    {
        _commandBatchBuilders.Add(new FuncProxy(x => buildQuery(x)));
        return this;
    }

    public CommandBatchBuilder Add(Func<ScopeContext, Option<string>> buildQuery)
    {
        _commandBatchBuilders.Add(new FuncProxy(buildQuery));
        return this;
    }

    public Option<string> Build(ScopeContext context)
    {
        Option<string>[] stats = _commandBatchBuilders.Select(x => x.BuildQuery(context)).ToArray();
        var error = stats.FirstOrDefault(x => x.IsError(), new Option<string>(StatusCode.OK));
        if (error.IsError()) return error;

        var cmds = stats.Select(x => x.Return()).Join(Environment.NewLine);
        return cmds;
    }

    private readonly struct FuncProxy : ICommandBatchBuilder
    {
        private readonly Func<ScopeContext, Option<string>> _buildQuery;
        public FuncProxy(Func<ScopeContext, Option<string>> buildQuery) => _buildQuery = buildQuery.NotNull();

        public Option<string> BuildQuery(ScopeContext context) => _buildQuery(context);
    }
}
