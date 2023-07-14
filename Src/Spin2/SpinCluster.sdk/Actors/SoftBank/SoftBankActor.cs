using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Types;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.SoftBank;

public interface ISoftBankActor : IGrainWithStringKey
{
    Task<SpinResponse> Delete(string traceId);
    Task<SpinResponse> Exist(string traceId);
}


public class SoftBankActor : Grain, ISoftBankActor
{
    private readonly IPersistentState<SoftBankModel> _state;
    private readonly ILogger<SoftBankActor> _logger;

    public SoftBankActor(
        [PersistentState(stateName: SpinConstants.Extension.SoftBank, storageName: SpinConstants.SpinStateStore)] IPersistentState<SoftBankModel> state,
        IValidator<SoftBankModel> validator,
        ILogger<SoftBankActor> logger
        )
    {
        _state = state;
        _logger = logger;
    }

    public Task<SpinResponse> Delete(string traceId) => _state.Delete<SoftBankModel>(this.GetPrimaryKeyString(), new ScopeContext(traceId, _logger).Location());
    public Task<SpinResponse> Exist(string traceId) => Task.FromResult(new SpinResponse(_state.RecordExists ? StatusCode.OK : StatusCode.NoContent));

    public async Task<SpinResponse> Create(SoftBankDetails softBankDetails0)
    {
        return null;
    }

    public async Task<SpinResponse> Add(SoftBankLedger softBankLedger)
    {
    }

    public async Task<SpinResponse<IReadOnlyList<SoftBankLedger>>> GetLedgerItems()
    {
    }

    public async Task<SpinResponse<SoftBankDetails>> GetBankDetails()
    {
    }

    public async Task<SpinResponse<decimal>> GetBalance()
    {
    }
}


public record SoftBankDetails
{
}

public record SoftBankLedger
{
}