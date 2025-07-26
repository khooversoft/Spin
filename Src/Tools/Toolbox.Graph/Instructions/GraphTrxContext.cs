using System.Collections.Immutable;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphTrxContext
{
    private int _queryNumber = -1;
    private readonly IGraphEngine _graphEngine;
    private readonly Sequence<QueryResult> _queryResult = new Sequence<QueryResult>();

    internal GraphTrxContext(string graphQuery, IEnumerable<IGraphInstruction> graphInstructions, IGraphEngine graphEngine, TransactionScope transactionScope, ScopeContext context)
    {
        GraphQuery = graphQuery.NotEmpty();
        Instructions = graphInstructions.NotNull().ToList();
        Cursor = new Cursor<IGraphInstruction>(Instructions);
        _graphEngine = graphEngine.NotNull();
        TransactionScope = transactionScope.NotNull();
        Context = context;
    }

    public string GraphQuery { get; }
    public List<IGraphInstruction> Instructions { get; }
    public Cursor<IGraphInstruction> Cursor { get; }
    public bool IsMutating => Instructions.Any(x => x is GiNode || x is GiEdge || x is GiDelete);
    public int NextQueryNumber() => Interlocked.Increment(ref _queryNumber);
    public IReadOnlyList<QueryResult> QueryResult => _queryResult;
    public JoinInstructionSwitch LastJoin { get; } = new JoinInstructionSwitch();
    public TransactionScope TransactionScope { get; }
    public ScopeContext Context { get; }
    public IDataClient<DataETag> DataClient => _graphEngine.DataClient;
    public GraphMap GetMap() => _graphEngine.DataManager.GetMap();

    public void AddQueryResult(Option subject) => _queryResult.Add(new QueryResult { QueryNumber = NextQueryNumber(), Option = subject });
    public void AddQueryResult(QueryResult subject) => _queryResult.Add(subject.NotNull() with
    {
        QueryNumber = NextQueryNumber(),
        Alias = subject.Alias ?? $"_alias_{_queryNumber}",
    });

    public void UpdateLastQueryResult(IEnumerable<GraphLinkData> graphLinkDataItems)
    {
        QueryResult? queryResult = this.GetLastQueryResult();
        if (queryResult == null || _queryResult.Count == 0) return;

        queryResult = queryResult with
        {
            DataLinks = graphLinkDataItems.ToImmutableArray(),
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
    public static bool IsMutating(this IReadOnlyList<IGraphInstruction> instructions) => instructions.NotNull().Any(x => x is GiNode || x is GiEdge || x is GiDelete);
    
    public static QueryBatchResult BuildQueryResult(this GraphTrxContext subject)
    {
        var result = new QueryBatchResult
        {
            GraphQuery = subject.GraphQuery,
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

    public static QueryResult? GetLastQueryResult(this GraphTrxContext subject) => subject.NotNull().QueryResult switch
    {
        { Count: 0 } => null,
        var v => subject.QueryResult[v.Count - 1],
    };
}