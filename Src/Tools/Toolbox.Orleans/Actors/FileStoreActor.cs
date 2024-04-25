using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans;

public interface IFileStoreActor : IGrainWithStringKey
{
    Task<Option<string>> Add(DataETag data, ScopeContext context);
    Task<Option> Exist(ScopeContext context);
    Task<Option> Delete(ScopeContext context);
    Task<Option<DataETag>> Get(ScopeContext context);
    Task<Option<string>> Set(DataETag data, ScopeContext context);
}

public class FileStoreActor : Grain, IFileStoreActor
{
    private readonly ILogger<DirectoryActor> _logger;
    private readonly ActorCacheState<DataETag> _state;

    public FileStoreActor(
        [PersistentState("json", OrleansConstants.StorageProviderName)] IPersistentState<DataETag> state,
        ILogger<DirectoryActor> logger
        )
    {
        _state = new ActorCacheState<DataETag>(state, TimeSpan.FromMinutes(15));
        _logger = logger.NotNull();
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.GetPrimaryKeyString().Assert(x => FileStoreTool.IsPathValid(x), x => $"ActorId={x} is not a valid path");
        await base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option<string>> Add(DataETag data, ScopeContext context)
    {
        context = context.With(_logger);
        if ((await _state.Exist()).IsOk())
        {
            context.LogError("Cannot add, ActorId={actorId} exist", this.GetPrimaryKeyString());
            return (StatusCode.Conflict, $"Cannot add, ActorId={this.GetPrimaryKeyString()} exist");
        }

        var result = (await _state.SetState(data)).LogStatus(context, "set state");
        if (result.IsError()) result.ToOptionStatus<string>();

        return _state.ETag;
    }

    public async Task<Option> Delete(ScopeContext context)
    {
        context = context.With(_logger);
        var clearOption = await _state.Clear();
        clearOption.LogStatus(context, "Clearing state for ActorId={actorId}", this.GetPrimaryKeyString());
        return clearOption;
    }

    public Task<Option> Exist(ScopeContext _) => _state.Exist();

    public async Task<Option<DataETag>> Get(ScopeContext _)
    {
        Option<DataETag> state = await _state.GetState();
        if (state.IsError()) return state;

        DataETag current = state.Return();
        return current.WithETag(_state.ETag);
    }

    public async Task<Option<string>> Set(DataETag data, ScopeContext context)
    {
        context = context.With(_logger);

        var result = await _state.SetState(data);
        result.LogStatus(context, "Set state for ActorId={actorId}", this.GetPrimaryKeyString());
        if (result.IsError()) return result.ToOptionStatus<string>();

        return _state.ETag;
    }
}