using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphQueryExecute : IGraphClient
{
    private readonly IGraphEngine _graphEngine;
    private readonly ILogger<GraphQueryExecute> _logger;
    private readonly string _instanceId = Guid.NewGuid().ToString();
    private readonly AsyncReaderWriterLock _rwLock = new AsyncReaderWriterLock();
    private readonly IGraphMapAccess _graphMapAccess;

    public GraphQueryExecute(IGraphEngine graphEngine, IGraphMapAccess graphMapAccess, ILogger<GraphQueryExecute> logger)
    {
        _graphEngine = graphEngine.NotNull();
        _graphMapAccess = graphMapAccess.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<QueryBatchResult>> ExecuteBatch(string command, ScopeContext context)
    {
        context = context.With(_logger);

        var result = await InternalExecute(_graphEngine, command, context).ConfigureAwait(false);
        return result;
    }

    public async Task<Option<QueryResult>> Execute(string command, ScopeContext context)
    {
        context = context.With(_logger);

        var result = await InternalExecute(_graphEngine, command, context).ConfigureAwait(false);
        if (result.IsError()) return result.ToOptionStatus<QueryResult>();

        return result.Return().Items.Last();
    }

    private async Task<Option<QueryBatchResult>> InternalExecute(IGraphEngine graphHost, string graphQuery, ScopeContext context)
    {
        await using var graphTrxContext = new GraphTrxContext(graphHost, context);

        var pContextOption = ParseQuery(graphQuery, graphTrxContext, context);
        if (pContextOption.IsError()) return pContextOption.ToOptionStatus<QueryBatchResult>();

        var graphQueryResultOption = await ExecuteInstruction(pContextOption.Return());
        if (graphQueryResultOption.IsError()) return graphQueryResultOption;
        var graphQueryResult = graphQueryResultOption.Return();

        return new Option<QueryBatchResult>(graphQueryResult, graphQueryResult.Option.StatusCode, graphQueryResult.Option.Error);
    }

    private static Option<QueryExecutionContext> ParseQuery(string graphQuery, IGraphTrxContext graphTrxContext, ScopeContext context)
    {
        using (var metric = context.LogDuration("queryExecution-parseQuery"))
        {
            graphTrxContext.Context.LogDebug("Parsing query: {graphQuery}", graphQuery);
            var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, graphTrxContext.Context);
            if (parse.Status.IsError()) return parse.Status.LogStatus(graphTrxContext.Context, graphQuery).ToOptionStatus<QueryExecutionContext>();

            var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs();
            var instructions = InterLangTool.Build(syntaxPairs);

            if (instructions.IsError()) return instructions
                .LogStatus(graphTrxContext.Context, "Parsing query: {graphQuery}", [graphQuery])
                .ToOptionStatus<QueryExecutionContext>();

            return new QueryExecutionContext(graphQuery, instructions.Return(), graphTrxContext);
        }
    }

    private async Task<Option<QueryBatchResult>> ExecuteInstruction(QueryExecutionContext pContext)
    {
        bool writeLockRequired = pContext.TrxContext.GraphEngine.IsShareMode || pContext.IsMutating;

        using var metric = pContext.TrxContext.Context.LogDuration($"queryExecution-executionInstruction, iKey={_instanceId}");
        using var releaseWriteReadLock = writeLockRequired ? (await _rwLock.WriterLockAsync()) : (await _rwLock.ReaderLockAsync());
        await using var scopeLease = await _graphMapAccess.CreateScope(pContext.TrxContext.Context);

        pContext.TrxContext.Context.LogDebug("Executing query: write={write}, graphQuery={graphQuery}, iKey={iKey}", writeLockRequired, pContext.GraphQuery, _instanceId);

        while (pContext.Cursor.TryGetValue(out var graphInstruction))
        {
            var metric2 = pContext.TrxContext.Context.LogDuration("queryExecution-executionInstruction-instruction");

            var queryResult = graphInstruction switch
            {
                GiNode giNode => await NodeInstruction.Process(giNode, pContext),
                GiEdge giEdge => EdgeInstruction.Process(giEdge, pContext),
                GiSelect giSelect => await SelectInstruction.Process(giSelect, pContext),
                GiDelete giDelete => await DeleteInstruction.Process(giDelete, pContext),
                _ => throw new UnreachableException(),
            };

            TimeSpan duration = metric2.Log();

            var itemResult = pContext.BuildQueryResult();

            if (queryResult.IsError())
            {
                pContext.TrxContext.Context.LogError("Graph batch failed - rolling back: query={graphQuery}, error={error}", pContext.TrxContext.Context, queryResult.ToString());
                await pContext.TrxContext.ChangeLog.Rollback();
                return itemResult;
            }
        }

        var batchResult = pContext.BuildQueryResult();

        if (pContext.IsMutating)
        {
            await pContext.TrxContext.ChangeLog.CommitLogs(pContext.TrxContext.Context);

            pContext.TrxContext.Context.LogDebug("Saving graph database, iKey={iKey}", _instanceId);
            var writeOption = await scopeLease.SaveDatabase(pContext.TrxContext.Context).ConfigureAwait(false);
            if (writeOption.IsError())
            {
                writeOption.LogStatus(pContext.TrxContext.Context, "Checkpoint failed - attempting to roll back");
                await pContext.TrxContext.ChangeLog.Rollback();

                return writeOption.ToOptionStatus<QueryBatchResult>();
            }
        }

        pContext.TrxContext.Context.LogDebug("Graph batch completed: query={graphQuery}, iKey={iKey}", pContext.GraphQuery, _instanceId);
        return batchResult;
    }
}
