using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    Task<Option> Set(ConfigModel model, string traceId);
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

        if (!_state.RecordExists)
        {
            await _state.ClearStateAsync();
            return StatusCode.NotFound;
        }

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

    public async Task<Option> Set(ConfigModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set config, actorKey={actorKey}", this.GetPrimaryKeyString());

        var v = new OptionTest()
            .Test(() => this.VerifyIdentity(model.ConfigId).LogResult(context.Location()))
            .Test(() => model.Validate());
        if (v.IsError()) return v.Option.LogResult(context.Location());

        _state.State = model;
        await _state.WriteStateAsync();

        return new Option(StatusCode.OK);
    }
}
