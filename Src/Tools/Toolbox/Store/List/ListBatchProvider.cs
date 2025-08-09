using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class ListBatchProvider<T> : IListStoreProvider<T>, IAsyncDisposable
{
    private readonly OperationQueue _operationQueue;
    private readonly BatchStream<AppendWork> _batchStream;
    private readonly ILogger<ListBatchProvider<T>> _logger;
    private readonly record struct AppendWork(string Key, IReadOnlyList<T> Data);

    public ListBatchProvider(IServiceProvider serviceProvider, TimeSpan batchInterval, ILogger<ListBatchProvider<T>> logger)
    {
        serviceProvider.NotNull();
        _logger = logger.NotNull();

        _operationQueue = new OperationQueue(5, serviceProvider.GetRequiredService<ILogger<OperationQueue>>());

        _batchStream = new BatchStream<AppendWork>(
            batchInterval,
            100,
            queueWork,
            serviceProvider.GetRequiredService<ILogger<BatchStream<AppendWork>>>()
        );

        async Task queueWork(IReadOnlyList<AppendWork> data)
        {
            ScopeContext context = _logger.ToScopeContext();
            context.LogDebug("Processing batch, count={count}", data.Count);

            foreach (var keyGroup in data.GroupBy(x => x.Key))
            {
                var values = keyGroup.SelectMany(x => x.Data).ToImmutableArray();
                context.LogDebug(
                    "Processing batch group={group}, groupCount={groupCount}, valuesCount={valuesCount}",
                    keyGroup.Key, keyGroup.Count(), values.Length
                    );

                await _operationQueue.Send(async () => await GetHandler().Append(keyGroup.Key, values, context), context);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _batchStream.DisposeAsync();
        await _operationQueue.DisposeAsync();
    }

    public IListStore<T>? InnerHandler { get; set; }

    public async Task<Option<string>> Append(string key, IEnumerable<T> data, ScopeContext context)
    {
        var appendData = new AppendWork(key, data.ToArray());
        await _batchStream.Send(appendData);

        //await _operationQueue.Send(async () => await GetHandler().Append(key, data, context), context);
        return "<deferred>";
    }

    public async Task<Option> Delete(string key, ScopeContext context)
    {
        await _batchStream.Drain();
        await _operationQueue.Send(async () => await GetHandler().Delete(key, context), context);
        return StatusCode.OK;
    }

    public async Task<Option<IReadOnlyList<T>>> Get(string key, ScopeContext context)
    {
        await _batchStream.Drain();
        return await _operationQueue.Get(async () => await GetHandler().Get(key, context), context);
    }

    public async Task<Option<IReadOnlyList<T>>> Get(string key, string pattern, ScopeContext context)
    {
        await _batchStream.Drain();
        return await _operationQueue.Get(async () => await GetHandler().Get(key, pattern, context), context);
    }

    public async Task<Option<IReadOnlyList<T>>> GetHistory(string key, DateTime timeIndex, ScopeContext context)
    {
        await _batchStream.Drain();
        return await _operationQueue.Get(async () => await GetHandler().GetHistory(key, timeIndex, context), context);
    }

    public async Task<IReadOnlyList<IStorePathDetail>> Search(string key, string pattern, ScopeContext context)
    {
        await _batchStream.Drain();
        return await _operationQueue.Get(async () => await GetHandler().Search(key, pattern, context), context);
    }

    private IListStore<T> GetHandler() => InnerHandler.NotNull("InnerHandler is not setup");
}
