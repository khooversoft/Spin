using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Logging;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class QueryExecution
{
    public static async Task<Option<QueryBatchResult>> Execute(IGraphHost graphHost, string graphQuery, ScopeContext context)
    {
        var runningState = (await graphHost.Run(context)).LogStatus(context, "Running graph host");
        if (runningState.IsError()) return runningState.LogStatus(context, "Failed to start Graph Host").ToOptionStatus<QueryBatchResult>();

        var trxContextOption = await graphHost.TransactionLog.StartTransaction(context);
        if (trxContextOption.IsError()) return trxContextOption.ToOptionStatus<QueryBatchResult>();
        var trxContext = trxContextOption.Return();

        var graphTrxContext = new GraphTrxContext(graphHost, trxContext, context);

        var pContextOption = ParseQuery(graphQuery, graphTrxContext);
        if (pContextOption.IsError()) return pContextOption.ToOptionStatus<QueryBatchResult>();

        var graphQueryResultOption = await ExecuteInstruction(pContextOption.Return());
        if (graphQueryResultOption.IsError()) return graphQueryResultOption;
        var graphQueryResult = graphQueryResultOption.Return();

        return new Option<QueryBatchResult>(graphQueryResult, graphQueryResult.Option.StatusCode, graphQueryResult.Option.Error);
    }

    private static Option<QueryExecutionContext> ParseQuery(string graphQuery, IGraphTrxContext graphTrxContext)
    {
        graphTrxContext.Context.LogInformation("Parsing query: {graphQuery}", graphQuery);
        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, graphTrxContext.Context);
        if (parse.Status.IsError()) return parse.Status.ToOptionStatus<QueryExecutionContext>();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs();
        var instructions = InterLangTool.Build(syntaxPairs);

        if (instructions.IsError()) return instructions
                .LogStatus(graphTrxContext.Context, "Parsing query: {graphQuery}", [graphQuery])
                .ToOptionStatus<QueryExecutionContext>();

        return new QueryExecutionContext(instructions.Return(), graphTrxContext);
    }

    private static async Task<Option<QueryBatchResult>> ExecuteInstruction(QueryExecutionContext pContext)
    {
        bool write = pContext.IsMutating;

        GraphMap map = pContext.TrxContext.Map;
        using (var release = write ? (await map.ReadWriterLock.WriterLockAsync()) : (await map.ReadWriterLock.ReaderLockAsync()))
        {
            while (pContext.Cursor.TryGetValue(out var graphInstruction))
            {
                await graphInstruction.CreateJournals().ForEachAsync(async x => await pContext.TrxContext.LogicalTrx.Write(x));

                var queryResult = graphInstruction switch
                {
                    GiNode giNode => await NodeInstruction.Process(giNode, pContext),
                    GiEdge giEdge => EdgeInstruction.Process(giEdge, pContext),
                    GiSelect giSelect => await SelectInstruction.Process(giSelect, pContext),
                    GiDelete giDelete => await DeleteInstruction.Process(giDelete, pContext),
                    _ => throw new UnreachableException(),
                };

                if (queryResult.IsError())
                {
                    pContext.TrxContext.Context.LogError("Graph batch failed - rolling back: query={graphQuery}, error={error}", pContext.TrxContext.Context, queryResult.ToString());
                    await pContext.TrxContext.LogicalTrx.RollbackTransaction();
                    await pContext.TrxContext.ChangeLog.Rollback();
                    return pContext.BuildQueryResult();
                }
            }

            if (write)
            {
                var writeOption = await pContext.TrxContext.CheckpointMap(pContext.TrxContext.Context);
                if (writeOption.IsError())
                {
                    writeOption.LogStatus(pContext.TrxContext.Context, "Checkpoint failed");
                    return (StatusCode.InternalServerError, "Checkpoint failed");
                }
            }

            await pContext.TrxContext.LogicalTrx.CommitTransaction();
            return pContext.BuildQueryResult();
        }
    }
}
