using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Configuration;

public interface IConfigActor : IGrainWithStringKey
{
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option<ConfigModel>> Get(string traceId);
    Task<Option> RemoveProperty(RemovePropertyModel model, string traceId);
    Task<Option> Set(ConfigModel model, string traceId);
    Task<Option> SetProperty(SetPropertyModel model, string traceId);
}

public class ConfigActor : Grain, IConfigActor
{
    private readonly IPersistentState<ConfigModel> _state;
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<ConfigActor> _logger;

    public ConfigActor(
        [PersistentState(stateName: SpinConstants.Extension.Json, storageName: SpinConstants.SpinStateStore)] IPersistentState<ConfigModel> state,
        IClusterClient clusterClient,
        ILogger<ConfigActor> logger
        )
    {
        _state = state.NotNull();
        _clusterClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema(SpinConstants.Schema.Config, new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting config, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (!_state.RecordExists)return StatusCode.NotFound;

        await _state.ClearStateAsync();
        return StatusCode.OK;
    }

    public Task<Option> Exist(string _) => new Option(_state.RecordExists ? StatusCode.OK : StatusCode.NotFound).ToTaskResult();

    public Task<Option<ConfigModel>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Get config, actorKey={actorKey}", this.GetPrimaryKeyString());

        var option = _state.RecordExists switch
        {
            true => _state.State.ToOption(),
            false => new Option<ConfigModel>(StatusCode.NotFound),
        };

        return option.ToTaskResult();
    }

    public async Task<Option> RemoveProperty(RemovePropertyModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Remove config, model={model}, actorKey={actorKey}", model, this.GetPrimaryKeyString());

        var test = new OptionTest()
            .Test(() => this.VerifyIdentity(model.ConfigId))
            .Test(() => _state.RecordExists, error: "No keyed configuration record present")
            .Test(() => model.Validate());
        if (test.IsError()) return test.Option;

        var dict = _state.State.Properties.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
        var removed = dict.Remove(model.Key);
        if (!removed) return StatusCode.NotFound;

        var update = _state.State with
        {
            Properties = dict
        };

        _state.State = update;
        await _state.WriteStateAsync();

        return StatusCode.OK;
    }

    public async Task<Option> Set(ConfigModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set config, model={model}, actorKey={actorKey}", model, this.GetPrimaryKeyString());

        var test = new OptionTest()
            .Test(() => this.VerifyIdentity(model.ConfigId))
            .Test(() => model.Validate());
        if (test.IsError()) return test.Option;

        _state.State = model;
        await _state.WriteStateAsync();

        return new Option(StatusCode.OK);
    }

    public async Task<Option> SetProperty(SetPropertyModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Adding property, model={model}, actorKey={actorKey}", model, this.GetPrimaryKeyString());

        var test = new OptionTest()
            .Test(() => this.VerifyIdentity(model.ConfigId))
            .Test(() => _state.RecordExists, error: "No keyed configuration record present")
            .Test(() => model.Validate());
        if (test.IsError()) return test.Option;

        var dict = _state.State.Properties.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
        dict[model.Key] = model.Value;

        var update = _state.State with
        {
            Properties = dict
        };

        _state.State = update;
        await _state.WriteStateAsync();

        return StatusCode.OK;
    }
}
