//using Microsoft.Extensions.Logging;
//using Orleans.Runtime;
//using SoftBank.sdk;
//using SpinCluster.sdk.Actors.ActorBase;
//using SpinCluster.sdk.Actors.User;
//using SpinCluster.sdk.Application;
//using SpinCluster.sdk.Types;
//using Toolbox.Data;
//using Toolbox.Tools.Validation;
//using Toolbox.Types;

//namespace SpinCluster.sdk.Actors.SoftBank;

//public interface ISoftBankActor : IGrainWithStringKey
//{
//    Task<SpinResponse> Delete(string traceId);
//    Task<SpinResponse> Exist(string traceId);
//}


//public class SoftBankActor : Grain, ISoftBankActor
//{
//    private readonly IPersistentState<BlobPackage> _state;
//    private readonly IValidator<BlobPackage> _validator;
//    private readonly ILogger<SoftBankActor> _logger;

//    public SoftBankActor(
//        [PersistentState(stateName: SpinConstants.Extension.SoftBank, storageName: SpinConstants.SpinStateStore)] IPersistentState<BlobPackage> state,
//        IValidator<BlobPackage> validator,
//        ILogger<SoftBankActor> logger
//        )
//    {
//        _state = state;
//        _validator = validator;
//        _logger = logger;
//    }

//    public Task<SpinResponse> Delete(string traceId) => _state.Delete<BlobPackage>(this.GetPrimaryKeyString(), new ScopeContext(traceId, _logger).Location());
//    public Task<SpinResponse> Exist(string traceId) => Task.FromResult(new SpinResponse(_state.RecordExists ? StatusCode.OK : StatusCode.NoContent));

//    public async Task<SpinResponse> Create(AccountDetail detail)
//    {
//        if (_state.RecordExists) return new SpinResponse(StatusCode.BadRequest);

//        var softBank = await SoftBankAccount.Create(detail.OwnerId, detail.ObjectId, _signCollection, _context).Return();

//        var accountDetail = new AccountDetail
//        {
//            ObjectId = _accountObjectId.ToString(),
//            OwnerId = _owner,
//            Name = "Softbank 1",
//        };

//        accountDetail.IsValid(_context.Location()).Should().BeTrue()
//    }

//    public async Task<SpinResponse> Add(LedgerItem softBankLedger)
//    {
//    }

//    public async Task<SpinResponse<IReadOnlyList<LedgerItem>>> GetLedgerItems()
//    {
//    }

//    public async Task<SpinResponse<AccountDetail>> GetBankDetails()
//    {
//    }

//    public async Task<SpinResponse<decimal>> GetBalance()
//    {
//    }

//    private Task<SpinResponse<BlobPackage>> Get(string traceId) => _state.Get<BlobPackage>(this.GetPrimaryKeyString(), new ScopeContext(traceId, _logger).Location());
//    private Task<SpinResponse> Set(BlobPackage model, string traceId) => _state.Set(model, this.GetPrimaryKeyString(), _validator, new ScopeContext(traceId, _logger).Location());

//}
