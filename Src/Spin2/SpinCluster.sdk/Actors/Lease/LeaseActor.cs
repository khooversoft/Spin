using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Lease;

public interface ILeaseActor : IGrainWithStringKey
{
    Task<Option<LeaseData>> Acquire(LeaseCreate model, string traceId);
    Task<Option<LeaseData>> Get(string leaseKey, string traceId);
    Task<Option> IsLeaseValid(string leaseKey, string traceId);
    Task<Option<IReadOnlyList<LeaseData>>> List(string traceId);
    Task<Option> Release(string leaseKey, string traceId);
}


public class LeaseActor : Grain, ILeaseActor
{
    private readonly IPersistentState<LeaseDataCollection> _state;
    private readonly ILogger _logger;

    public LeaseActor(
        [PersistentState(stateName: SpinConstants.Extension.Json, storageName: SpinConstants.SpinStateStore)] IPersistentState<LeaseDataCollection> state,
        ILogger<LeaseActor> logger
        )
    {
        _state = state;
        _logger = logger;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.GetPrimaryKeyString().Assert(x => x == SpinConstants.LeaseActorKey, x => $"Actor key {x} is invalid, must match {SpinConstants.LeaseActorKey}");
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option<LeaseData>> Acquire(LeaseCreate model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (!model.Validate(out var v)) return v.ToOptionStatus<LeaseData>();
        context.Location().LogInformation("Acquire lease, model={model}", model);

        _state.State = _state.RecordExists ? _state.State : new LeaseDataCollection();

        var leaseData = new LeaseData
        {
            LeaseKey = model.LeaseKey,
            Payload = model.Payload,
            TimeToLive = model.TimeToLive,
        };

        var addOption = _state.State.TryAdd(leaseData);
        if (addOption.IsError())
        {
            context.Location().LogError("Lease is already active for leaseKey={leaseKey}", model.LeaseKey);
            return addOption.ToOptionStatus<LeaseData>();
        }

        _state.State.Cleanup();
        await _state.WriteStateAsync();

        context.Location().LogInformation("Acquiring lease for actorKey={actorKey}, leaseKey={leaseKey}", this.GetPrimaryKeyString(), leaseData.LeaseKey);
        return leaseData;
    }

    public async Task<Option<LeaseData>> Get(string leaseKey, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Get lease, leaseKey={leaseKey}", leaseKey);
        if (!IdPatterns.IsName(leaseKey)) return StatusCode.BadRequest;

        if (!_state.RecordExists) return StatusCode.NotFound;
        if (_state.State.Cleanup()) await _state.WriteStateAsync();

        Option<LeaseData> getOption = _state.State.Get(leaseKey);
        return getOption;
    }

    public async Task<Option> IsLeaseValid(string leaseKey, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Releasing lease request for id={id}, leaseKey={leaseKey}", this.GetPrimaryKeyString(), leaseKey);

        if (!_state.RecordExists) return StatusCode.NotFound;
        if (_state.State.Cleanup()) await _state.WriteStateAsync();

        Option<LeaseData> getOption = _state.State.Get(leaseKey);
        return getOption.ToOptionStatus();
    }

    public async Task<Option<IReadOnlyList<LeaseData>>> List(string _)
    {
        if (!_state.RecordExists) return StatusCode.NotFound;
        if (_state.State.Cleanup()) await _state.WriteStateAsync();

        var response = _state.State.Leases.Values
            .Where(x => x.IsActive())
            .ToArray();

        return response;
    }

    public async Task<Option> Release(string leaseKey, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Releasing lease request for leaseKey={leaseKey}", this.GetPrimaryKeyString(), leaseKey);

        if (!_state.RecordExists) return StatusCode.NotFound;

        bool cleaned = _state.State.Cleanup();
        bool removed = _state.State.Remove(leaseKey);

        if (cleaned || removed) await _state.WriteStateAsync();
        return StatusCode.OK;
    }
}
