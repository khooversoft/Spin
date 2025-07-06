using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

internal class QueueStoreHandler : IDataProvider
{
    private readonly ILogger<QueueStoreHandler> _logger;
    private const string _name = nameof(QueueStoreHandler);
    private readonly OperationQueue _operationQueue;

    public QueueStoreHandler(IServiceProvider serviceProvider, ILogger<QueueStoreHandler> logger)
    {
        _logger = logger.NotNull();
        _operationQueue = new OperationQueue(1000, serviceProvider.NotNull().GetRequiredService<ILogger<OperationQueue>>());
    }

    public IDataProvider? InnerHandler { get; set; }
    public DataProviderCounters Counters { get; } = new();


    public async Task<Option<DataPipelineContext>> Execute(DataPipelineContext dataContext, ScopeContext context)
    {
        dataContext.NotNull().Validate().ThrowOnError();
        context = context.With(_logger);
        context.Location().LogDebug("Execute: Executing command={command}, name={name}", dataContext.Command, _name);

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
                    context.LogDebug("Send completed for command={command}, name={name}", dataContext.Command, _name);
                }, context);
                return dataContext with { Queued = true };

            case DataPipelineCommand.Get:
            case DataPipelineCommand.GetList:
            case DataPipelineCommand.DeleteList:
                var result = await _operationQueue.Get(async () =>
                {
                    var result = await ((IDataProvider)this).NextExecute(dataContext, context);
                    context.LogDebug("Get list complete for command={command}, name={name}", dataContext.Command, _name);
                    return result;
                }, context);
                return result;

            case DataPipelineCommand.Drain:
                context.LogDebug("Draining, name={name}", _name);
                await _operationQueue.Drain(context);
                return dataContext with { Queued = true };
        }

        var nextOption = await ((IDataProvider)this).NextExecute(dataContext, context).ConfigureAwait(false);
        return nextOption;
    }
}

