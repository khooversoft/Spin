using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans;

public interface IFileStoreActor : IGrainWithStringKey
{
    Task<Option<string>> Add(DataETag data, string traceId);
    Task<Option> Exist(string traceId);
    Task<Option> Delete(string traceId);
    Task<Option<DataETag>> Get(string traceId);
    Task<Option<string>> Set(DataETag data, string traceId);
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

    public async Task<Option<string>> Add(DataETag data, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if ((await _state.Exist()).IsOk())
        {
            context.LogError("Cannot add, ActorId={actorId} exist", this.GetPrimaryKeyString());
            return (StatusCode.Conflict, $"Cannot add, ActorId={this.GetPrimaryKeyString()} exist");
        }

        var result = (await _state.SetState(data)).LogStatus(context, "set state");
        if (result.IsError()) result.ToOptionStatus<string>();

        return _state.ETag;
    }

    public async Task<Option> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        var clearOption = await _state.Clear();
        clearOption.LogStatus(context, "Clearing state for ActorId={actorId}", this.GetPrimaryKeyString());
        return clearOption;
    }

    public Task<Option> Exist(string traceId) => _state.Exist();
    public async Task<Option<DataETag>> Get(string traceId)
    {
        Option<DataETag> state = await _state.GetState();
        if (state.IsError()) return state;

        return state.Return().WithETag(_state.ETag);
    }


    public async Task<Option<string>> Set(DataETag data, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        var result = await _state.SetState(data);
        result.LogStatus(context, "Set state for ActorId={actorId}", this.GetPrimaryKeyString());
        if (result.IsError()) return result.ToOptionStatus<string>();

        return _state.ETag;
    }
}