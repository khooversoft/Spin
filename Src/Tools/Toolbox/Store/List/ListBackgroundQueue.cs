using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class ListBackgroundQueue<T> : IListStoreProvider<T>
{
    private readonly OperationQueue _operationQueue;
    private readonly ILogger<ListBackgroundQueue<T>> _logger;

    public ListBackgroundQueue(IServiceProvider serviceProvider, ILogger<ListBackgroundQueue<T>> logger)
    {
        serviceProvider.NotNull();
        _logger = logger.NotNull();

        _operationQueue = new OperationQueue(1000, serviceProvider.GetRequiredService<ILogger<OperationQueue>>());
    }

    public IListStore<T>? InnerHandler { get; set; }

    public async Task<Option<string>> Append(string key, IEnumerable<T> data, ScopeContext context)
    {
        await _operationQueue.Send(async () => await GetHandler().Append(key, data, context), context);
        return "<deferred>";
    }

    public async Task<Option> Delete(string key, ScopeContext context)
    {
        await _operationQueue.Send(async () => await GetHandler().Delete(key, context), context);
        return StatusCode.OK;
    }

    public async Task<Option<IReadOnlyList<T>>> Get(string key, ScopeContext context) =>
        await _operationQueue.Get(async () => await GetHandler().Get(key, context), context);

    public async Task<Option<IReadOnlyList<T>>> Get(string key, string pattern, ScopeContext context) =>
        await _operationQueue.Get(async () => await GetHandler().Get(key, pattern, context), context);

    public async Task<Option<IReadOnlyList<T>>> GetHistory(string key, DateTime timeIndex, ScopeContext context) =>
        await _operationQueue.Get(async () => await GetHandler().GetHistory(key, timeIndex, context), context);

    public async Task<Option<IReadOnlyList<T>>> GetPartition(string key, DateTime timeIndex, ScopeContext context) =>
        await _operationQueue.Get(async () => await GetHandler().GetPartition(key, timeIndex, context), context);

    public async Task<IReadOnlyList<IStorePathDetail>> Search(string key, string pattern, ScopeContext context) =>
        await _operationQueue.Get(async () => await GetHandler().Search(key, pattern, context), context);

    private IListStore<T> GetHandler() => InnerHandler.NotNull("InnerHandler is not setup");
}