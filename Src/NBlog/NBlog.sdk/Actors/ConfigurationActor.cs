﻿using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public interface IConfigurationActor : IGrainWithStringKey
{
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option<NBlogConfiguration>> Get(string traceId);
    Task<Option> Set(NBlogConfiguration model, string traceId);
    Task<IReadOnlyList<IndexGroup>> Lookup(IReadOnlyList<string> groupName, string traceId);
}

public class ConfigurationActor : Grain, IConfigurationActor
{
    private readonly ActorCacheState<NBlogConfiguration> _state;
    private readonly ILogger<ConfigurationActor> _logger;

    public ConfigurationActor(
        StateManagement stateManagement,
        [PersistentState("default", NBlogConstants.DataLakeProviderName)] IPersistentState<NBlogConfiguration> state,
        ILogger<ConfigurationActor> logger
        )
    {
        _logger = logger.NotNull();
        stateManagement.NotNull();

        _state = new ActorCacheState<NBlogConfiguration>(stateManagement, state, TimeSpan.FromMinutes(15));
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.GetPrimaryKeyString().Assert(NBlogConstants.Tool.IsConfigurationActorKey, x => $"ActorKey={x} is not valid.");

        _state.SetName(nameof(ConfigurationActor), this.GetPrimaryKeyString());
        VerifyDbName();
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

        return await _state.GetState(context);
    }

    public async Task<Option> Set(NBlogConfiguration model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set NBlogConfiguration, actorKey={actorKey}", this.GetPrimaryKeyString());
        if (!model.Validate(out var v1)) return v1;

        var fileIdTest = model.VerifyFileId(this.GetPrimaryKeyString(), context);
        if (fileIdTest.IsError()) return fileIdTest;

        return await _state.SetState(model, context);
    }

    public async Task<IReadOnlyList<IndexGroup>> Lookup(IReadOnlyList<string> groupNames, string traceId)
    {
        var context = new ScopeContext(_logger);

        var configOption = await _state.GetState(context);
        if (configOption.IsError())
        {
            context.Location().LogError("Failed to get state, error={error}", configOption.ToString());
            return Array.Empty<IndexGroup>();
        }

        NBlogConfiguration config = configOption.Return();

        groupNames = groupNames.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        var result = config.IndexGroups
            .Join(groupNames, x => x.GroupName, x => x, (o, i) => o, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return result;
    }

    private void VerifyDbName()
    {
        if (!_state.RecordExists) return;

        var option = _state.State.VerifyFileId(this.GetPrimaryKeyString(), new ScopeContext(_logger));
        option.ThrowOnError();
    }
}
