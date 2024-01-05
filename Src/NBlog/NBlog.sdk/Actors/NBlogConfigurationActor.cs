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
    private readonly ActorCacheState<NBlogConfiguration> _state;
    private readonly ILogger<NBlogConfigurationActor> _logger;

    public NBlogConfigurationActor([PersistentState("default", NBlogConstants.DataLakeProviderName)] IPersistentState<NBlogConfiguration> state, ILogger<NBlogConfigurationActor> logger)
    {
        _logger = logger.NotNull();
        _state = new ActorCacheState<NBlogConfiguration>(state, TimeSpan.FromMinutes(15));
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        string actorKey = this.GetPrimaryKeyString();
        if (actorKey.EqualsIgnoreCase(NBlogConstants.ConfigurationActorKey)) throw new ArgumentException($"ActorKey={actorKey} should be {NBlogConstants.ConfigurationActorKey}");
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting NBlogConfiguration, actorKey={actorKey}", this.GetPrimaryKeyString());

        return await _state.Clear();
    }

    public Task<Option> Exist(string traceId) => _state.Exist();

    public async Task<Option<NBlogConfiguration>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Get NBlogConfiguration, actorKey={actorKey}", this.GetPrimaryKeyString());

        return await _state.GetState();
    }

    public async Task<Option> Set(NBlogConfiguration model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set NBlogConfiguration, actorKey={actorKey}", this.GetPrimaryKeyString());
        if (!model.Validate(out var v1)) return v1;

        return await _state.SetState(model);
    }
}
