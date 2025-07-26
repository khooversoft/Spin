using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Models;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class TransactionScope : IAsyncDisposable
{
    private readonly Func<DataChangeRecord, ScopeContext, Task<Option>> _commitFunc;
    private readonly Func<DataChangeRecord, ScopeContext, Task<Option>> _rollback;
    private readonly ILogger _logger;
    private readonly ConcurrentQueue<DataChangeEntry> _queue = new();
    private bool _isCommitted = false;

    internal TransactionScope(
        Func<DataChangeRecord, ScopeContext, Task<Option>> commitFunc,
        Func<DataChangeRecord, ScopeContext, Task<Option>> rollback,
        LogSequenceNumber logSequenceNumber,
        ILogger logger
        )
    {
        _commitFunc = commitFunc.NotNull();
        _rollback = rollback.NotNull();
        _logger = logger.NotNull();

        LogSequenceNumber = logSequenceNumber.NotNull();
    }

    public string TransactionId { get; } = Guid.NewGuid().ToString();
    public LogSequenceNumber LogSequenceNumber { get; }

    public void Enqueue(DataChangeEntry entry) => _queue.Enqueue(entry);

    public async Task<Option> Commit(ScopeContext context)
    {
        context = context.With(_logger);

        var committed = Interlocked.CompareExchange(ref _isCommitted, true, false);
        if (committed) throw new InvalidOperationException();

        context.LogDebug("Committing changes for transaction");
        var commitOption = await _commitFunc(GetChangeRecords(), context).ConfigureAwait(false);
        if (commitOption.IsError())
        {
            commitOption.LogStatus(context, "Failed to commit changes");
            throw new InvalidOperationException($"Failed to commit changes: statusCode={commitOption.StatusCode}, error={commitOption.Error}");
        }

        _queue.Clear();
        return commitOption;
    }

    public async Task Rollback(ScopeContext context)
    {
        var current = Interlocked.CompareExchange(ref _isCommitted, true, false);
        if (current)
        {
            if (_queue.Count > 0) throw new InvalidOperationException($"Commit or rollback already executed, queue.count={_queue.Count}");
            return;
        }

        if (_queue.Count == 0) return;

        context.LogDebug("Rolling back changes for transaction");
        var rollbackOption = await _rollback(GetChangeRecords(), context);
        if (rollbackOption.IsError())
        {
            context.LogError("Failed to rollback changes: {Error}", rollbackOption.Error);
            throw new InvalidOperationException($"Failed to rollback changes: {rollbackOption.Error}");
        }

        _queue.Clear();
    }

    public async ValueTask DisposeAsync() => await Rollback(_logger.ToScopeContext()).ConfigureAwait(false);

    private DataChangeRecord GetChangeRecords() => new DataChangeRecord
    {
        TransactionId = TransactionId,
        Entries = _queue.ToImmutableArray()
    };
}


public static class TransactionScopeExtensions
{
    public static void NodeAdd(this TransactionScope subject, GraphNode newNode) =>
        subject.Enqueue<GraphNode>(ChangeSource.Node, newNode.Key, ChangeOperation.Add, null, newNode.ToDataETag());
    public static void NodeDelete(this TransactionScope subject, GraphNode currentNode) =>
        subject.Enqueue<GraphNode>(ChangeSource.Node, currentNode.Key, ChangeOperation.Delete, currentNode.ToDataETag(), null);
    public static void NodeChange(this TransactionScope subject, GraphNode currentNode, GraphNode updatedNode) =>
        subject.Enqueue<GraphNode>(ChangeSource.Node, currentNode.Key, ChangeOperation.Delete, currentNode.ToDataETag(), updatedNode.ToDataETag());


    public static void EdgeAdd(this TransactionScope subject, GraphEdge newNode) =>
        subject.Enqueue<GraphEdge>(ChangeSource.Edge, newNode.Key, ChangeOperation.Add, null, newNode.ToDataETag());

    public static void EdgeDelete(this TransactionScope subject, GraphEdge currentNode) =>
        subject.Enqueue<GraphEdge>(ChangeSource.Edge, currentNode.Key, ChangeOperation.Delete, currentNode.ToDataETag(), null);

    public static void EdgeChange(this TransactionScope subject, GraphEdge currentNode, GraphEdge updatedNode) =>
        subject.Enqueue<GraphEdge>(ChangeSource.Edge, currentNode.Key, ChangeOperation.Delete, currentNode.ToDataETag(), updatedNode.ToDataETag());


    public static void DataAdd(this TransactionScope subject, string fileId, DataETag newData) =>
        subject.Enqueue<DataETag>(ChangeSource.Data, fileId, ChangeOperation.Add, null, newData);

    public static void DataDelete(this TransactionScope subject, string fileId, DataETag currentData) =>
        subject.Enqueue<GraphNode>(ChangeSource.Data, fileId, ChangeOperation.Delete, currentData, null);

    public static void DataChange(this TransactionScope subject, string fileId, DataETag currentNode, DataETag updatedNode) =>
        subject.Enqueue<DataETag>(ChangeSource.Data, fileId, ChangeOperation.Delete, currentNode, updatedNode);



    private static void Enqueue<T>(this TransactionScope subject, string sourceName, string objectId, string action, DataETag? before, DataETag? after)
    {
        subject.NotNull();

        var entry = new DataChangeEntry
        {
            LogSequenceNumber = subject.LogSequenceNumber.Next(),
            TransactionId = subject.TransactionId,
            Date = DateTime.UtcNow,
            TypeName = typeof(T).Name,
            SourceName = sourceName.NotEmpty(),
            ObjectId = objectId.NotEmpty(),
            Action = action,
            Before = before,
            After = after,
        };

        subject.Enqueue(entry);
    }
}