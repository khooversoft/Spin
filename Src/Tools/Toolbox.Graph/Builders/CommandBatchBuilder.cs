using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface ICommandBatchBuilder
{
    Option<string> BuildQuery(ILogger logger);
}

public class CommandBatchBuilder
{
    private readonly Sequence<ICommandBatchBuilder> _commandBatchBuilders = new();

    public CommandBatchBuilder Add(ICommandBatchBuilder commandBatchBuilder)
    {
        _commandBatchBuilders.Add(commandBatchBuilder.NotNull());
        return this;
    }

    public CommandBatchBuilder Add(Func<ILogger, string> buildQuery)
    {
        _commandBatchBuilders.Add(new FuncProxy(x => buildQuery(x)));
        return this;
    }

    public CommandBatchBuilder Add(Func<ILogger, Option<string>> buildQuery)
    {
        _commandBatchBuilders.Add(new FuncProxy(buildQuery));
        return this;
    }

    public Option<string> Build(ILogger logger)
    {
        Option<string>[] stats = _commandBatchBuilders.Select(x => x.BuildQuery(logger)).ToArray();
        var error = stats.FirstOrDefault(x => x.IsError(), new Option<string>(StatusCode.OK));
        if (error.IsError()) return error;

        var cmds = stats.Select(x => x.Return()).Join(Environment.NewLine);
        return cmds;
    }

    private readonly struct FuncProxy : ICommandBatchBuilder
    {
        private readonly Func<ILogger, Option<string>> _buildQuery;
        public FuncProxy(Func<ILogger, Option<string>> buildQuery) => _buildQuery = buildQuery.NotNull();

        public Option<string> BuildQuery(ILogger logger) => _buildQuery(logger);
    }
}
