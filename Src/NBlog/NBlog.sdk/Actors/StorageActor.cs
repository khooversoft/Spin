using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Toolbox.Orleans;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public interface IStorageActor : IGrainWithStringKey
{
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option<DataETag>> Get(string traceId);
    Task<Option> Set(DataETag model, string traceId);
}


public class StorageActor : Grain, IStorageActor
{
    private readonly ILogger<StorageActor> _logger;
    private ActorCacheState<DataETag> _state;

    public StorageActor(
        StateManagement stateManagement,
        [PersistentState("default", NBlogConstants.DataLakeProviderName)] IPersistentState<DataETag> state,
        ILogger<StorageActor> logger
        )
    {
        state.NotNull();
        _logger = logger.NotNull();
        stateManagement.NotNull();

        _state = new ActorCacheState<DataETag>(stateManagement, state, TimeSpan.FromMinutes(15));
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        string actorKey = this.GetPrimaryKeyString();
        FileId.Create(actorKey).ThrowOnError("Actor Id is invalid");

        _state.SetName(nameof(StorageActor), this.GetPrimaryKeyString());
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting storage, actorKey={actorKey}", this.GetPrimaryKeyString());

        return await _state.Clear();
    }

    public Task<Option> Exist(string traceId) => _state.Exist();

    public async Task<Option<DataETag>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Get ArticleManifest, actorKey={actorKey}", this.GetPrimaryKeyString());

        return await _state.GetState(context);
    }

    public async Task<Option> Set(DataETag model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set ArticleManifest, actorKey={actorKey}", this.GetPrimaryKeyString());

        string actorKey = this.GetPrimaryKeyString();
        if (!model.Validate(out var v1)) return v1;

        return await _state.SetState(model, context);
    }
}
