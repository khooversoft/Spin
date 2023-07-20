using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SoftBank.sdk;
using SoftBank.sdk.Models;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Orleans.Types;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;


namespace SpinCluster.sdk.Actors.SoftBank;

public interface ISoftBankActor : IGrainWithStringKey
{
    Task<SpinResponse> Delete(string traceId);
    Task<SpinResponse> Exist(string traceId);
    Task<SpinResponse> Add(LedgerItem ledgerItem, string traceId);
    Task<SpinResponse> Set(AccountDetail accountDetail, string traceId);
}

public class SoftBankActor : Grain, ISoftBankActor
{
    private readonly IPersistentState<BlobPackage> _state;
    private readonly IValidator<BlobPackage> _blobPackageValidator;
    private readonly IValidator<AccountDetail> _accountValidator;
    private readonly ILogger<SoftBankActor> _logger;
    private readonly ISign _sign;
    private readonly IValidator<LedgerItem> _ledgerItemValidator;
    private CacheObject<SoftBankAccount> _softBankCache = new CacheObject<SoftBankAccount>(TimeSpan.FromMinutes(15));
    private ScopeContext _actorContext;

    public SoftBankActor(
        [PersistentState(stateName: SpinConstants.Extension.SmartContract, storageName: SpinConstants.SpinStateStore)] IPersistentState<BlobPackage> state,
        IValidator<BlobPackage> blobPackageValidator,
        IValidator<AccountDetail> accountValidator,
        IValidator<LedgerItem> ledgerItemValidator,
        ISign sign,
        ILogger<SoftBankActor> logger
        )
    {
        _state = state.NotNull();
        _blobPackageValidator = blobPackageValidator.NotNull();
        _accountValidator = accountValidator.NotNull();
        _logger = logger.NotNull();
        _sign = sign.NotNull();
        _ledgerItemValidator = ledgerItemValidator.NotNull();

        _actorContext = new ScopeContext(_logger);
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _softBankCache.Clear();
        await GetSoftBankAccount(_actorContext);

        await base.OnActivateAsync(cancellationToken);
    }

    public Task<SpinResponse> Delete(string traceId) => _state.Delete(this.GetPrimaryKeyString(), new ScopeContext(traceId, _logger).Location());
    public Task<SpinResponse> Exist(string traceId) => new SpinResponse(_state.RecordExists ? StatusCode.OK : StatusCode.NoContent).ToTaskResult();

    public async Task<SpinResponse> Create(AccountDetail detail, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        if (_state.RecordExists) return new SpinResponse(StatusCode.BadRequest);

        Option<ValidatorResult> validator = _accountValidator.Validate(detail);
        if (validator.IsError()) return validator.Return().ToSpinResponse();

        var softBank = await SoftBankAccount.Create(detail.OwnerId, detail.ObjectId.ToObjectId(), _sign, context).Return();

        var accountDetail = new AccountDetail
        {
            ObjectId = detail.ObjectId.ToString(),
            OwnerId = detail.OwnerId,
            Name = detail.Name,
        };

        accountDetail.IsValid(context.Location()).Assert(x => x == true, "Invalid account detail");

        _state.State = softBank.ToBlobPackage();
        await _state.WriteStateAsync();

        _softBankCache.Set(softBank);

        return new SpinResponse(StatusCode.OK);
    }

    public async Task<SpinResponse> Set(AccountDetail accountDetail, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set account detail={accountDetail}", accountDetail);

        Option<ValidatorResult> validator = _accountValidator.Validate(accountDetail, context.Location());
        if (validator.IsError()) return validator.Return().ToSpinResponse();

        Option<SoftBankAccount> softBankAccount = await GetSoftBankAccount(context);
        if (softBankAccount.IsError()) return softBankAccount.ToSpinResponse();

        BlockScalarStream<AccountDetail> stream = softBankAccount.Return().GetAccountDetailStream();
        Option<DataBlock> blockData = await stream.CreateDataBlock(accountDetail, accountDetail.OwnerId).Sign(_sign, context);
        if (blockData.IsError()) return blockData.ToSpinResponse();

        stream.Add(blockData.Return());

        return new SpinResponse(StatusCode.OK);
    }

    public async Task<SpinResponse> Add(LedgerItem ledgerItem, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        Option<ValidatorResult> validator = _ledgerItemValidator.Validate(ledgerItem, context.Location());
        if (validator.IsError()) return validator.Return().ToSpinResponse();

        Option<SoftBankAccount> softBankAccount = await GetSoftBankAccount(context);
        if (softBankAccount.IsError()) return softBankAccount.ToSpinResponse();

        BlockStream<LedgerItem> stream = softBankAccount.Return().GetLedgerStream();
        Option<DataBlock> blockData = await stream.CreateDataBlock(ledgerItem, ledgerItem.OwnerId).Sign(_sign, context);
        if (blockData.IsError()) return blockData.ToSpinResponse();

        stream.Add(blockData.Return());

        return new SpinResponse(StatusCode.OK);
    }

    public async Task<SpinResponse<AccountDetail>> GetBankDetails(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        Option<SoftBankAccount> softBankAccount = await GetSoftBankAccount(context);
        if (softBankAccount.IsError()) return softBankAccount.ToSpinResponse<AccountDetail>();

        BlockScalarStream<AccountDetail> stream = softBankAccount.Return().GetAccountDetailStream();
        var accountDetail = stream.Get();
        if (accountDetail.IsNoContent()) return new SpinResponse<AccountDetail>(StatusCode.NotFound);

        return new SpinResponse<AccountDetail>(accountDetail.Return());
    }

    public async Task<SpinResponse<decimal>> GetBalance(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        Option<SoftBankAccount> softBankAccount = await GetSoftBankAccount(context);
        if (softBankAccount.IsError()) return softBankAccount.ToSpinResponse<decimal>();

        return new SpinResponse<decimal>(softBankAccount.Return().GetBalance());
    }

    public async Task<SpinResponse<IReadOnlyList<LedgerItem>>> GetLedgerItems(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        Option<SoftBankAccount> softBankAccount = await GetSoftBankAccount(context);
        if (softBankAccount.IsError()) return softBankAccount.ToSpinResponse<IReadOnlyList<LedgerItem>>();

        BlockStream<LedgerItem> stream = softBankAccount.Return().GetLedgerStream();
        var list = stream.Get();

        return new SpinResponse<IReadOnlyList<LedgerItem>>(list);
    }

    private async Task<Option<SoftBankAccount>> GetSoftBankAccount(ScopeContext context)
    {
        if (_softBankCache.TryGetValue(out SoftBankAccount value)) return value;

        context.Location().LogInformation("Reading BLOB for SoftBankAccount, actorKey={actorKey}", this.GetPrimaryKeyString());
        await _state.ReadStateAsync();

        Option<SoftBankAccount> acct = _state.State.ToSoftBankAccount(_actorContext).LogResult(_actorContext.Location());
        if (acct.IsError())
        {
            context.Location().LogError("Failed to read/find BLOB for SoftBankAccount, actorKey={actorKey}", this.GetPrimaryKeyString());
            return acct;
        }

        SoftBankAccount softBankAccount = acct.Return();
        _softBankCache.Set(softBankAccount);

        return softBankAccount;
    }

    private Task<SpinResponse<BlobPackage>> Get(string traceId) => _state.Get<BlobPackage>(this.GetPrimaryKeyString(), new ScopeContext(traceId, _logger).Location());
    private Task<SpinResponse> Set(BlobPackage model, string traceId) => _state.Set(model, this.GetPrimaryKeyString(), _blobPackageValidator, new ScopeContext(traceId, _logger).Location());

}
