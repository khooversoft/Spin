using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class QueryExecution
{
    public static async Task<Option<QueryBatchResult>> ExecuteBatch(this IGraphEngine graphEngine, string command, ScopeContext context)
    {
        var result = await InternalExecute(graphEngine, command, context).ConfigureAwait(false);
        return result;
    }

    public static async Task<Option<QueryResult>> Execute(this IGraphEngine graphEngine, string command, ScopeContext context)
    {
        graphEngine.NotNull();

        var result = await InternalExecute(graphEngine, command, context).ConfigureAwait(false);
        if (result.IsError()) return result.ToOptionStatus<QueryResult>();

        return result.Return().Items.Last();
    }

    private static async Task<Option<QueryBatchResult>> InternalExecute(IGraphEngine graphHost, string graphQuery, ScopeContext context)
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
            graphTrxContext.Context.LogTrace("Parsing query: {graphQuery}", graphQuery);
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

    private static async Task<Option<QueryBatchResult>> ExecuteInstruction(QueryExecutionContext pContext)
    {
        bool write = pContext.IsMutating;

        GraphMap map = pContext.TrxContext.Map;

        using var metric = pContext.TrxContext.Context.LogDuration("queryExecution-executionInstruction");
        using var release = write ? (await map.ReadWriterLock.WriterLockAsync()) : (await map.ReadWriterLock.ReaderLockAsync());

        var leaseOption = await pContext.TrxContext.AcquireScope();
        if (leaseOption.IsError()) return leaseOption.ToOptionStatus<QueryBatchResult>();

        await using var leaseScope = leaseOption.Return();

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

        if (write)
        {
            await pContext.TrxContext.ChangeLog.CommitLogs();

            var writeOption = await pContext.TrxContext.CheckpointMap();
            if (writeOption.IsError())
            {
                writeOption.LogStatus(pContext.TrxContext.Context, "Checkpoint failed");
                return (StatusCode.InternalServerError, "Checkpoint failed");
            }
        }

        return batchResult;
    }
}
