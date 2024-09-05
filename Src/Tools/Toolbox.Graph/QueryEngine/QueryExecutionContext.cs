using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal class QueryExecutionContext
{
    private int _queryNumber = -1;
    public QueryExecutionContext(IEnumerable<IGraphInstruction> graphInstructions, IGraphTrxContext graphContext)
    {
        Instructions = graphInstructions.NotNull().ToList();
        Cursor = new Cursor<IGraphInstruction>(Instructions);
        GraphContext = graphContext.NotNull();
    }

    public List<IGraphInstruction> Instructions { get; }
    public Cursor<IGraphInstruction> Cursor { get; }
    public IGraphTrxContext GraphContext { get; }
    public bool IsMutating => Instructions.Any(x => x is GiNode || x is GiEdge || x is GiDelete);
    public int NextQueryNumber() => Interlocked.Increment(ref _queryNumber);
    public Sequence<QueryResult> DataSets { get; init; } = new Sequence<QueryResult>();
    public Sequence<QueryTrace> Traces { get; } = new Sequence<QueryTrace>();

    public void AddTrace(Option option) => Traces.Add(new QueryTrace
    {
        QueryNumber = NextQueryNumber(),
        Option = option,
    });
}

internal record QueryTrace
{
    public int QueryNumber { get; init; }
    public Option Option { get; init; }
}

internal static class QueryExecutionContextTool
{
    public static QueryBatchResult BuildQueryResult(this QueryExecutionContext subject)
    {
        var result = new QueryBatchResult
        {
            Option = subject.Traces.LastOrDefault(x => x.Option.IsError()).Func(x => x != null ? x.Option : new Option(StatusCode.OK)),
            Items = subject.DataSets.ToImmutableArray(),
        };

        return result;
    }
}