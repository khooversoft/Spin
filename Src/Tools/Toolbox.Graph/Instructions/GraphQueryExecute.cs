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
    private readonly GraphMapDataManager _graphDataManager;

    public GraphQueryExecute(IGraphEngine graphEngine, GraphMapDataManager graphDataManager, ILogger<GraphQueryExecute> logger)
    {
        _graphEngine = graphEngine.NotNull();
        _graphDataManager = graphDataManager.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<QueryBatchResult>> ExecuteBatch(string command)
    {
        var result = await InternalExecute(_graphEngine, command);
        return result;
    }

    public async Task<Option<QueryResult>> Execute(string command)
    {
        var result = await InternalExecute(_graphEngine, command);
        if (result.IsError()) return result.ToOptionStatus<QueryResult>();

        return result.Return().Items.Last();
    }

    private async Task<Option<QueryBatchResult>> InternalExecute(IGraphEngine graphEngine, string graphQuery)
    {
        var pContextOption = ParseQuery(graphQuery, graphEngine);
        if (pContextOption.IsError()) return pContextOption.ToOptionStatus<QueryBatchResult>();

        var graphQueryResultOption = await ExecuteInstruction(graphQuery, pContextOption.Return(), graphEngine);
        if (graphQueryResultOption.IsError()) return graphQueryResultOption;
        var graphQueryResult = graphQueryResultOption.Return();

        return new Option<QueryBatchResult>(graphQueryResult, graphQueryResult.Option.StatusCode, graphQueryResult.Option.Error);
    }

    private Option<IReadOnlyList<IGraphInstruction>> ParseQuery(string graphQuery, IGraphEngine graphEngine)
    {
        using (_logger.LogDuration("queryExecution-parseQuery"))
        {
            _logger.LogDebug("Parsing query: {graphQuery}", graphQuery);
            var parse = _graphEngine.LanguageParser.Parse(graphQuery);
            if (parse.Status.IsError()) return _logger.LogStatus(parse.Status, graphQuery).ToOptionStatus<IReadOnlyList<IGraphInstruction>>();

            var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs();
            Option<IReadOnlyList<IGraphInstruction>> instructions = InterLangTool.Build(syntaxPairs);
            _logger.LogStatus(instructions, "Parsing query: {graphQuery}", [graphQuery]);

            return instructions;
        }
    }

    private async Task<Option<QueryBatchResult>> ExecuteInstruction(string graphQuery, IReadOnlyList<IGraphInstruction> instructions, IGraphEngine graphEngine)
    {
        graphQuery.NotEmpty();
        instructions.NotNull();
        graphEngine.NotNull();
        bool isMutating = instructions.IsMutating();

        _logger.LogDebug("Executing query: write={write}, graphQuery={graphQuery}, iKey={iKey}", isMutating, graphQuery, _instanceId);
        using var metric = _logger.LogDuration($"queryExecution-executionInstruction, iKey={_instanceId}");

        using var releaseWriteReadLock = isMutating ? (await _rwLock.WriterLockAsync()) : (await _rwLock.ReaderLockAsync());
        await using var scopeLease = _graphDataManager.StartTransaction();

        var pContext = new GraphTrxContext(graphQuery, instructions, graphEngine, scopeLease, _logger);

        while (pContext.Cursor.TryGetValue(out var graphInstruction))
        {
            var metric2 = _logger.LogDuration("queryExecution-executionInstruction-instruction");

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
                _logger.LogStatus(queryResult, "Graph batch failed - rolling back: query={graphQuery}", [graphQuery]);
                _logger.LogError("Graph batch failed - rolling back: query={graphQuery}, error={error}", graphQuery, queryResult.ToString());
                await scopeLease.Rollback();
                return itemResult;
            }
        }

        var batchResult = pContext.BuildQueryResult();

        if (pContext.IsMutating)
        {
            _logger.LogDebug("Committing transaction: query={graphQuery}, iKey={iKey}", pContext.GraphQuery, _instanceId);
            var commitOption = await pContext.TransactionScope.Commit();
            _logger.LogStatus(commitOption, "Commit transaction: query={graphQuery}, iKey={iKey}", [pContext.GraphQuery, _instanceId]);

            if (commitOption.IsError())
            {
                _logger.LogError("Failed to commit, Checkpoint failed - attempting to roll back");
                await pContext.TransactionScope.Rollback();
                return commitOption.ToOptionStatus<QueryBatchResult>();
            }
        }

        _logger.LogDebug("Graph batch completed: query={graphQuery}, iKey={iKey}", pContext.GraphQuery, _instanceId);
        return batchResult;
    }
}
