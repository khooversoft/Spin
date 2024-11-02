using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal class QueryExecutionContext
{
    private int _queryNumber = -1;
    private readonly Sequence<QueryResult> _queryResult = new Sequence<QueryResult>();

    public QueryExecutionContext(IEnumerable<IGraphInstruction> graphInstructions, IGraphTrxContext graphContext)
    {
        Instructions = graphInstructions.NotNull().ToList();
        Cursor = new Cursor<IGraphInstruction>(Instructions);
        TrxContext = graphContext.NotNull();
    }

    public List<IGraphInstruction> Instructions { get; }
    public Cursor<IGraphInstruction> Cursor { get; }
    public IGraphTrxContext TrxContext { get; }
    public bool IsMutating => Instructions.Any(x => x is GiNode || x is GiEdge || x is GiDelete);
    public int NextQueryNumber() => Interlocked.Increment(ref _queryNumber);
    public IReadOnlyList<QueryResult> QueryResult => _queryResult;
    public JoinInstructionSwitch LastJoin { get; } = new JoinInstructionSwitch();

    public void AddQueryResult(Option subject) => _queryResult.Add(new QueryResult { QueryNumber = NextQueryNumber(), Option = subject });
    public void AddQueryResult(QueryResult subject) => _queryResult.Add(subject.NotNull() with
    {
        QueryNumber = NextQueryNumber(),
        Alias = subject.Alias ?? $"_alias_{_queryNumber}",
    });

    public void UpdateLastQueryResult(IEnumerable<GraphLinkData> graphLinkDatas)
    {
        QueryResult? queryResult = this.GetLastQueryResult();
        if (queryResult == null || _queryResult.Count == 0) return;

        queryResult = queryResult with
        {
            DataLinks = graphLinkDatas.ToImmutableArray(),
        };

        _queryResult[_queryResult.Count - 1] = queryResult;
    }


    public class JoinInstructionSwitch
    {
        private ISelectInstruction? _lastJoinInstruction;
        public void Set(ISelectInstruction instruction) => Interlocked.Exchange(ref _lastJoinInstruction, instruction);
        public ISelectInstruction? GetAndClear() => Interlocked.Exchange(ref _lastJoinInstruction, null);
    }
}

internal static class QueryExecutionContextTool
{
    public static QueryBatchResult BuildQueryResult(this QueryExecutionContext subject)
    {
        var result = new QueryBatchResult
        {
            Option = subject.QueryResult.LastOrDefault(x => x.Option.IsError()).Func(x => x != null ? x.Option : new Option(StatusCode.OK)),
            Items = subject.QueryResult switch
            {
                { Count: 0 } => Array.Empty<QueryResult>(),
                { Count: 1 } => subject.QueryResult.ToImmutableArray(),

                _ => subject.QueryResult
                    .Take(subject.QueryResult.Count - 1)
                    .Where(x => x.Alias == null || !x.Alias.StartsWith('_'))
                    .Append(subject.QueryResult.Last())
                    .ToImmutableArray(),
            },
        };

        return result;
    }

    public static QueryResult? GetLastQueryResult(this QueryExecutionContext subject) => subject.NotNull().QueryResult switch
    {
        { Count: 0 } => null,
        var v => subject.QueryResult[v.Count - 1],
    };
}