using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public interface INBlogConfigurationActor : IGrainWithStringKey
{
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option<NBlogConfiguration>> Get(string traceId);
    Task<Option> Set(NBlogConfiguration model, string traceId);
}

public class NBlogConfigurationActor : Grain, INBlogConfigurationActor
{
    private readonly TimeSpan _cacheTime = TimeSpan.FromMinutes(15);
    private readonly IPersistentState<NBlogConfiguration> _state;
    private readonly ILogger<NBlogConfigurationActor> _logger;
    private DateTime _nextRead;

    public NBlogConfigurationActor([PersistentState(stateName: "default")] IPersistentState<NBlogConfiguration> state, ILogger<NBlogConfigurationActor> logger)
    {
        _state = state.NotNull();
        _logger = logger.NotNull();

        ResetNextRead();
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        string actorKey = this.GetPrimaryKeyString();
        if (actorKey.EqualsIgnoreCase(NBlogConstants.ConfigurationActorId)) throw new ArgumentException($"ActorKey={actorKey} should be {NBlogConstants.ConfigurationActorId}");
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting NBlogConfiguration, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (!_state.RecordExists) return StatusCode.NotFound;

        await _state.ClearStateAsync();

        return StatusCode.OK;
    }

    public Task<Option> Exist(string traceId) => new Option(_state.RecordExists ? StatusCode.OK : StatusCode.NotFound).ToTaskResult();

    public async Task<Option<NBlogConfiguration>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Get NBlogConfiguration, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (DateTime.UtcNow > _nextRead && _state.RecordExists)
        {
            ResetNextRead();
            await _state.ReadStateAsync();
        }

        var option = _state.RecordExists switch
        {
            true => _state.State,
            false => new Option<NBlogConfiguration>(StatusCode.NotFound),
        };

        return option;
    }

    public async Task<Option> Set(NBlogConfiguration model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set NBlogConfiguration, actorKey={actorKey}", this.GetPrimaryKeyString());
        if (!model.Validate(out var v1)) return v1;

        _state.State = model;
        await _state.WriteStateAsync();

        ResetNextRead();
        return StatusCode.OK;
    }

    private void ResetNextRead() => _nextRead = DateTime.UtcNow.Add(_cacheTime);
}
