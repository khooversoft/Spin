using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class QueryExecution
{
    public static async Task<Option<QueryBatchResult>> Execute(IGraphTrxContext graphContext, string graphQuery)
    {
        var pContextOption = ParseQuery(graphQuery, graphContext);
        if (pContextOption.IsError()) return pContextOption.ToOptionStatus<QueryBatchResult>();

        var graphQueryResult = await ExecuteInstruction(pContextOption.Return());
        return graphQueryResult;
    }

    private static Option<QueryExecutionContext> ParseQuery(string graphQuery, IGraphTrxContext graphContext)
    {
        graphContext.Context.LogInformation("Parsing query: {graphQuery}", graphQuery);
        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, graphContext.Context);
        if (parse.Status.IsError()) return parse.Status.ToOptionStatus<QueryExecutionContext>();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        if (instructions.IsError()) return instructions.LogStatus(graphContext.Context, $"Parsing query: {graphQuery}").ToOptionStatus<QueryExecutionContext>();

        return new QueryExecutionContext(instructions.Return(), graphContext);
    }

    private static async Task<QueryBatchResult> ExecuteInstruction(QueryExecutionContext pContext)
    {
        bool write = pContext.IsMutating;

        GraphMap map = pContext.GraphContext.Map;
        using (var release = write ? (await map.ReadWriterLock.WriterLockAsync()) : (await map.ReadWriterLock.ReaderLockAsync()))
        {
            while (pContext.Cursor.TryGetValue(out var graphInstruction))
            {
                var queryResult = graphInstruction switch
                {
                    GiNode giNode => await NodeInstruction.Process(giNode, pContext),
                    GiSelect giSelect => await SelectInstruction.Process(giSelect, pContext),
                    GiDelete giDelete => await DeleteInstruction.Process(giDelete, pContext),
                    _ => throw new UnreachableException(),
                };

                if (queryResult.IsError())
                {
                    pContext.GraphContext.Context.LogError("Graph batch failed - rolling back: query={graphQuery}, error={error}", pContext.GraphContext.Context, queryResult.ToString());
                    pContext.GraphContext.ChangeLog.Rollback();
                    break;
                }
            }
        }

        return pContext.BuildQueryResult();
    }
}
