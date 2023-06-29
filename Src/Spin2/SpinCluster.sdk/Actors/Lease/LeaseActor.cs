using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Types;
using Toolbox.Extensions;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Lease;

public interface ILeaseActor : IGrainWithStringKey
{
    Task<SpinResponse<LeaseData>> Acquire(string traceId);
    Task<StatusCode> Release(string leaseId, string traceId);
}


public class LeaseActor : Grain, ILeaseActor
{
    private readonly IPersistentState<LeaseData> _state;
    private readonly ILogger _logger;

    public LeaseActor(
        [PersistentState(stateName: "leaseV1", storageName: SpinConstants.SpinStateStore)] IPersistentState<LeaseData> state,
        ILogger<LeaseActor> logger
        )
    {
        _state = state;
        _logger = logger;
    }

    public async Task<SpinResponse<LeaseData>> Acquire(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        if (_state.RecordExists && _state.State.IsLeaseValid()) return new SpinResponse<LeaseData>(StatusCode.Conflict);

        var leaseData = new LeaseData
        {
            LeaseId = Guid.NewGuid().ToString(),
            ObjectId = this.GetPrimaryKeyString(),
        };

        _state.State = leaseData;
        await _state.WriteStateAsync();

        context.Location().LogInformation("Acquiring lease for id={id}, leaseId={leaseId}, leaseData={leaseData}",
            this.GetPrimaryKeyString(), leaseData.LeaseId, leaseData.ToJsonPascalSafe(context));

        return leaseData;
    }

    public async Task<StatusCode> Release(string leaseId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Releasing lease request for id={id}, leaseId={leaseId}", this.GetPrimaryKeyString(), leaseId);

        if (!_state.RecordExists) return StatusCode.NotFound;

        if (!_state.State.IsLeaseValid())
        {
            await _state.ClearStateAsync();
            return StatusCode.NotFound;
        }

        if (_state.State.LeaseId != leaseId) return StatusCode.Conflict;

        context.Location().LogInformation("Releasing lease for id={id}, leaseId={leaseId}", this.GetPrimaryKeyString(), leaseId);
        await _state.ClearStateAsync();

        return StatusCode.OK;
    }
}
