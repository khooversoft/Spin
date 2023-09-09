﻿using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.Smartc;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Models;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Agent;

public interface IAgentActor : IGrainWithStringKey
{
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option<AgentModel>> Get(string traceId);
    Task<Option> IsActive(string traceId);
    Task<Option> Set(AgentModel model, string traceId);
    Task<Option<AgentAssignmentModel>> GetAssignment(string traceId);
}

public class AgentActor : Grain, IAgentActor
{
    private readonly IPersistentState<AgentModel> _state;
    private readonly ILogger<ContractActor> _logger;
    private readonly IClusterClient _clusterClient;

    public AgentActor(
        [PersistentState(stateName: "default", storageName: SpinConstants.SpinStateStore)] IPersistentState<AgentModel> state,
        IClusterClient clusterClient,
        ILogger<ContractActor> logger
        )
    {
        _state = state.NotNull();
        _clusterClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema(SpinConstants.Schema.Agent, new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting agent, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (!_state.RecordExists)
        {
            await _state.ClearStateAsync();
            return StatusCode.NotFound;
        }

        context.Location().LogInformation("Deleted agent, actorKey={actorKey}", this.GetPrimaryKeyString());
        await _state.ClearStateAsync();

        return StatusCode.OK;
    }

    public async Task<Option> Exist(string traceId)
    {
        await _state.ReadStateAsync();
        return _state.RecordExists ? StatusCode.OK : StatusCode.NotFound;
    }

    public Task<Option<AgentModel>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Get agent, actorKey={actorKey}", this.GetPrimaryKeyString());

        var option = _state.RecordExists switch
        {
            true => _state.State,
            false => new Option<AgentModel>(StatusCode.NotFound),
        };

        return option.ToTaskResult();
    }

    public async Task<Option<AgentAssignmentModel>> GetAssignment(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Get assignment, actorKey={actorKey}", this.GetPrimaryKeyString());

        Option<ScheduleWorkModel> assignOption = await _clusterClient
            .GetResourceGrain<ISchedulerActor>(SpinConstants.Scheduler)
            .AssignWork(this.GetPrimaryKeyString(), context.TraceId);

        if (assignOption.IsError()) return assignOption.ToOptionStatus<AgentAssignmentModel>();

        var model = new AgentAssignmentModel
        {
            AgentId = this.GetPrimaryKeyString(),
            WorkId = this.GetPrimaryKeyString(),
            SmartcId = assignOption.Return().SmartcId,
            Command = assignOption.Return().Command,
        };

        var smartcOption = await _clusterClient
            .GetResourceGrain<ISmartcActor>(assignOption.Return().SmartcId)
            .SetAssignment(model, context.TraceId);

        if (smartcOption.IsError()) return smartcOption.ToOptionStatus<AgentAssignmentModel>();

        return model;
    }

    public async Task<Option> IsActive(string traceId)
    {
        await _state.ReadStateAsync();
        if (!_state.RecordExists) return StatusCode.NotFound;

        return _state.State.Enabled ? StatusCode.OK : StatusCode.Unauthorized;
    }

    public async Task<Option> Set(AgentModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set agent, actorKey={actorKey}", this.GetPrimaryKeyString());

        var test = new OptionTest()
            .Test(() => this.VerifyIdentity(model.AgentId).LogResult(context.Location()))
            .Test(() => model.Validate().LogResult(context.Location()));
        if (test.IsError()) return test;

        _state.State = model;
        await _state.WriteStateAsync();

        return StatusCode.OK;
    }
}

