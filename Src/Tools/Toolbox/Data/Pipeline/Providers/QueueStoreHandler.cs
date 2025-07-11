using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

internal class QueueStoreHandler : IDataProvider, IAsyncDisposable
{
    private readonly ILogger<QueueStoreHandler> _logger;
    private readonly OperationQueue _operationQueue;

    public QueueStoreHandler(IServiceProvider serviceProvider, ILogger<QueueStoreHandler> logger)
    {
        serviceProvider.NotNull();
        _logger = logger.NotNull();

        _operationQueue = new OperationQueue(1000, serviceProvider.GetRequiredService<ILogger<OperationQueue>>());
    }

    public IDataProvider? InnerHandler { get; set; }
    public DataProviderCounters Counters { get; } = new();


    public async Task<Option<DataPipelineContext>> Execute(DataPipelineContext dataContext, ScopeContext context)
    {
        dataContext.NotNull().Validate().ThrowOnError();
        context = context.With(_logger);
        context.Location().LogDebug("Execute: Executing command={command}", dataContext.Command);

        dataContext.NotNull();
        switch (dataContext.Command)
        {
            case DataPipelineCommand.Append:
            case DataPipelineCommand.AppendList:
            case DataPipelineCommand.Delete:
            case DataPipelineCommand.Set:
                await _operationQueue.Send(async () =>
                {
                    await ((IDataProvider)this).NextExecute(dataContext, context);
                    context.LogDebug("Send completed for command={command}", dataContext.Command);
                }, context);
                return dataContext;

            case DataPipelineCommand.Get:
            case DataPipelineCommand.GetList:
            case DataPipelineCommand.DeleteList:
                var result = await _operationQueue.Get(async () =>
                {
                    var result = await ((IDataProvider)this).NextExecute(dataContext, context);
                    context.LogDebug("Get list complete for command={command}", dataContext.Command);
                    return result;
                }, context);

                return result;

            case DataPipelineCommand.Drain:
                context.LogDebug("Draining");
                await _operationQueue.Drain(context);
                return dataContext;
        }

        var nextOption = await ((IDataProvider)this).NextExecute(dataContext, context).ConfigureAwait(false);
        return nextOption;
    }

    public async ValueTask DisposeAsync() => await _operationQueue.DisposeAsync();
}

