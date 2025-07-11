using Microsoft.Extensions.Logging;
using SpinCluster.abstraction;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Lease;

public interface ILeaseActor : IGrainWithStringKey
{
    Task<Option> Acquire(LeaseData lease, string traceId);
    Task<Option<LeaseData>> Get(string leaseKey, string traceId);
    Task<Option> IsLeaseValid(string leaseKey, string traceId);
    Task<Option<IReadOnlyList<LeaseData>>> List(QueryParameter query, string traceId);
    Task<Option> Release(string leaseKey, string traceId);
}

public static class LeaseActorExtensions
{
    public static ILeaseActor GetLeaseActor(this IClusterClient clusterClient) => clusterClient
        .GetResourceGrain<ILeaseActor>(SpinConstants
        .LeaseActorKey);
}


public class LeaseActor : Grain, ILeaseActor
{
    private readonly IPersistentState<LeaseDataCollection> _state;
    private readonly ILogger _logger;

    public LeaseActor(
        [PersistentState(stateName: SpinConstants.Ext.Json, storageName: SpinConstants.SpinStateStore)] IPersistentState<LeaseDataCollection> state,
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

    public async Task<Option> Acquire(LeaseData leaseData, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (!leaseData.Validate(out var v)) return v;
        context.Location().LogInformation("Acquire lease, model={model}", leaseData);

        _state.State = _state.RecordExists ? _state.State : new LeaseDataCollection();

        var addOption = _state.State.TryAdd(leaseData);
        if (addOption.IsError())
        {
            context.Location().LogError("Lease is already active for leaseKey={leaseKey}", leaseData.LeaseKey);
            return addOption;
        }

        _state.State.Cleanup();
        await _state.WriteStateAsync();

        context.Location().LogInformation("Acquiring lease for actorKey={actorKey}, leaseKey={leaseKey}", this.GetPrimaryKeyString(), leaseData.LeaseKey);
        return StatusCode.OK;
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

    public async Task<Option<IReadOnlyList<LeaseData>>> List(QueryParameter query, string traceId)
    {
        if (!_state.RecordExists) return StatusCode.NotFound;
        if (_state.State.Cleanup()) await _state.WriteStateAsync();

        var response = _state.State.Leases.Values
            .Skip(query.Index)
            .Where(x => x.IsActive() && isMatch(query.Filter, x.Reference))
            .Take(query.Count)
            .ToArray();

        return response;

        bool isMatch(string? filter, string? reference) => (filter, reference) switch
        {
            (null, null) => true,
            (null, string) v => true,
            (string, null) v => false,
            (string f, string r) v => f == r,
        };
    }

    public async Task<Option> Release(string leaseKey, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Releasing lease request for leaseKey={leaseKey}", this.GetPrimaryKeyString(), leaseKey);

        if (!_state.RecordExists) return StatusCode.NotFound;

        bool cleaned = _state.State.Cleanup();
        bool removed = _state.State.Remove(leaseKey);

        if (cleaned || removed) await _state.WriteStateAsync();
        return removed ? StatusCode.OK : StatusCode.NotFound;
    }
}
