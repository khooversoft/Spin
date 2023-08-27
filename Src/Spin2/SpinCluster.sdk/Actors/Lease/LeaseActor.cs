using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Lease;

public interface ILeaseActor : IGrainWithStringKey
{
    Task<Option<LeaseData>> Acquire(LeaseCreate model, string traceId);
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
        this.VerifySchema(SpinConstants.Schema.Lease, new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option<LeaseData>> Acquire(LeaseCreate model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        var v = model.Validate();
        if (v.IsError()) return v.ToOptionStatus<LeaseData>();

        _state.State = _state.RecordExists ? _state.State : new LeaseDataCollection();

        if (_state.State.LeaseData.TryGetValue(model.LeaseKey, out LeaseData? readLeaseData))
        {
            if (readLeaseData.IsLeaseValid()) return StatusCode.Conflict;
        }

        var leaseData = new LeaseData
        {
            LeaseKey = model.LeaseKey,
            AccountId = this.GetPrimaryKeyString(),
            Payload = model.Payload,
            TimeToLive = model.TimeToLive,
        };

        _state.State.LeaseData[model.LeaseKey] = leaseData;
        await _state.WriteStateAsync();

        context.Location().LogInformation("Acquiring lease for actorKey={actorKey}, leaseId={leaseId}, leaseKey={leaseKey}",
            this.GetPrimaryKeyString(), leaseData.LeaseId, leaseData.LeaseKey);

        return leaseData;
    }

    public async Task<Option> Release(string leaseKey, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Releasing lease request for id={id}, leaseKey={leaseKey}", this.GetPrimaryKeyString(), leaseKey);

        var test = new Option()
            .Test(() => _state.RecordExists)
            .Test(() => _state.State.LeaseData.Remove(leaseKey));
        if (test.IsError()) return StatusCode.NotFound;

        await _state.WriteStateAsync();
        return StatusCode.OK;
    }

    public Task<Option> IsLeaseValid(string leaseKey, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Releasing lease request for id={id}, leaseKey={leaseKey}", this.GetPrimaryKeyString(), leaseKey);

        Option option = _state.RecordExists switch
        {
            false => StatusCode.NotFound,
            true => _state.State.LeaseData.TryGetValue(leaseKey, out LeaseData? readLeaseData) switch
            {
                false => StatusCode.NotFound,
                true => readLeaseData.IsLeaseValid() switch
                {
                    true => StatusCode.OK,
                    false => StatusCode.Conflict,
                }
            }
        };

        return option.ToTaskResult();
    }

    public Task<Option<IReadOnlyList<LeaseData>>> List(string traceId)
    {
        Option<IReadOnlyList<LeaseData>> response = _state.RecordExists switch
        {
            false => Array.Empty<LeaseData>(),

            true => _state.State.LeaseData.Values
                .Where(x => x.IsLeaseValid())
                .ToArray(),
        };

        return response.ToTaskResult();
    }

    public class LeaseDataCollection
    {
        public IDictionary<string, LeaseData> LeaseData { get; set; } = new Dictionary<string, LeaseData>();
    };
}
