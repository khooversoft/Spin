﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Smartc;

public interface ISmartcActor : IGrainWithStringKey
{
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option<SmartcModel>> Get(string traceId);
    Task<Option> Set(SmartcModel model, string traceId);
}

public class SmartcActor : Grain, ISmartcActor
{
    private readonly IPersistentState<SmartcModel> _state;
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<ContractActor> _logger;

    public SmartcActor(
        [PersistentState(stateName: "default", storageName: SpinConstants.SpinStateStore)] IPersistentState<SmartcModel> state,
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
        this.VerifySchema(SpinConstants.Schema.Smartc, new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting Smartc, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (!_state.RecordExists)
        {
            await _state.ClearStateAsync();
            return StatusCode.NotFound;
        }

        context.Location().LogInformation("Deleted Smartc, actorKey={actorKey}", this.GetPrimaryKeyString());
        await _state.ClearStateAsync();

        return StatusCode.OK;
    }

    public async Task<Option> Exist(string traceId)
    {
        await _state.ReadStateAsync();
        return _state.RecordExists ? StatusCode.OK : StatusCode.NotFound;
    }

    public Task<Option<SmartcModel>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Get Smartc, actorKey={actorKey}", this.GetPrimaryKeyString());

        var option = _state.RecordExists switch
        {
            true => _state.State,
            false => new Option<SmartcModel>(StatusCode.NotFound),
        };

        return option.ToTaskResult();
    }

    public async Task<Option> Set(SmartcModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set Smartc, actorKey={actorKey}", this.GetPrimaryKeyString());

        var test = new OptionTest()
            .Test(() => this.VerifyIdentity(model.SmartcId).LogResult(context.Location()))
            .Test(() => model.Validate().LogResult(context.Location()));
        if (test.IsError()) return test;

        _state.State = model;
        await _state.WriteStateAsync();

        return StatusCode.OK;
    }
}

