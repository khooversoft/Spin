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

public class SoftBankActor : Grain, ISoftBankActor
{
    private readonly IPersistentState<BlobPackage> _state;
    private readonly IValidator<AccountDetail> _accountValidator;
    private readonly ILogger<SoftBankActor> _logger;
    private readonly ISign _sign;
    private readonly IValidator<LedgerItem> _ledgerItemValidator;
    private readonly ISignValidate _signValidate;
    private ScopeContext _actorContext;

    public SoftBankActor(
        [PersistentState(stateName: SpinConstants.Extension.SmartContract, storageName: SpinConstants.SpinStateStore)] IPersistentState<BlobPackage> state,
        IValidator<AccountDetail> accountValidator,
        IValidator<LedgerItem> ledgerItemValidator,
        ISign sign,
        ISignValidate signValidate,
        ILogger<SoftBankActor> logger
        )
    {
        _state = state.NotNull();
        _accountValidator = accountValidator.NotNull();
        _logger = logger.NotNull();
        _sign = sign.NotNull();
        _ledgerItemValidator = ledgerItemValidator.NotNull();
        _signValidate = signValidate.NotNull();

        _actorContext = new ScopeContext(_logger);
    }

    public Task<SpinResponse> Delete(string traceId) => _state.Delete(this.GetPrimaryKeyString(), new ScopeContext(traceId, _logger).Location());
    public Task<SpinResponse> Exist(string traceId) => new SpinResponse(_state.RecordExists ? StatusCode.OK : StatusCode.NoContent).ToTaskResult();

    public async Task<SpinResponse> Create(AccountDetail detail, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Creating contract accountDetail={accountDetail}", detail);
        detail.IsValid(context.Location()).Assert(x => x == true, "Invalid account detail");

        if (_state.RecordExists)
        {
            context.Location().LogInformation("Cannot create contract, already exist, actorId={actorId}", this.GetPrimaryKeyString());
            return new SpinResponse(StatusCode.BadRequest, $"Cannot create contract, already exist, actorId={this.GetPrimaryKeyString()}");
        }

        Option<ValidatorResult> validator = _accountValidator.Validate(detail);
        if (validator.IsError()) return validator.Return().ToSpinResponse();

        var softBank = await SoftBankAccount.Create(detail.OwnerId, detail.ObjectId.ToObjectId(), _sign, context).Return();

        BlockScalarStream<AccountDetail> stream = softBank.GetAccountDetailStream();
        var datablock = await stream.CreateDataBlock(detail, detail.OwnerId).Sign(_sign, context);
        if( datablock.IsError()) return datablock.ToSpinResponse();
        stream.Add(datablock.Return());

        return await WriteContract(softBank, context);
    }

    public async Task<SpinResponse> SetAccountDetail(AccountDetail detail, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set account detail={accountDetail}", detail);

        Option<ValidatorResult> validator = _accountValidator.Validate(detail, context.Location());
        if (validator.IsError()) return validator.Return().ToSpinResponse();

        Option<SoftBankAccount> softBank = await ReadContract(context);
        if (softBank.IsError()) return softBank.ToSpinResponse();

        BlockScalarStream<AccountDetail> stream = softBank.Return().GetAccountDetailStream();
        Option<DataBlock> blockData = await stream.CreateDataBlock(detail, detail.OwnerId).Sign(_sign, context);
        if (blockData.IsError()) return blockData.ToSpinResponse();

        stream.Add(blockData.Return());

        return await WriteContract(softBank.Return(), context);
    }

    public async Task<SpinResponse> AddLedgerItem(LedgerItem ledgerItem, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        Option<ValidatorResult> validator = _ledgerItemValidator.Validate(ledgerItem, context.Location());
        if (validator.IsError()) return validator.Return().ToSpinResponse();

        Option<SoftBankAccount> softBank = await ReadContract(context);
        if (softBank.IsError()) return softBank.ToSpinResponse();

        BlockStream<LedgerItem> stream = softBank.Return().GetLedgerStream();
        Option<DataBlock> blockData = await stream.CreateDataBlock(ledgerItem, ledgerItem.OwnerId).Sign(_sign, context);
        if (blockData.IsError()) return blockData.ToSpinResponse();

        stream.Add(blockData.Return());

        return await WriteContract(softBank.Return(), context);
    }

    public async Task<SpinResponse<AccountDetail>> GetBankDetails(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        Option<SoftBankAccount> softBankAccount = await ReadContract(context);
        if (softBankAccount.IsError()) return softBankAccount.ToSpinResponse<AccountDetail>();

        BlockScalarStream<AccountDetail> stream = softBankAccount.Return().GetAccountDetailStream();
        var accountDetail = stream.Get();
        if (accountDetail.IsNoContent()) return new SpinResponse<AccountDetail>(StatusCode.NotFound);

        return new SpinResponse<AccountDetail>(accountDetail.Return());
    }

    public async Task<SpinResponse<decimal>> GetBalance(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        Option<SoftBankAccount> softBankAccount = await ReadContract(context);
        if (softBankAccount.IsError()) return softBankAccount.ToSpinResponse<decimal>();

        return new SpinResponse<decimal>(softBankAccount.Return().GetBalance());
    }

    public async Task<SpinResponse<IReadOnlyList<LedgerItem>>> GetLedgerItems(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        Option<SoftBankAccount> softBank = await ReadContract(context);
        if (softBank.IsError()) return softBank.ToSpinResponse<IReadOnlyList<LedgerItem>>();

        BlockStream<LedgerItem> stream = softBank.Return().GetLedgerStream();
        var list = stream.Get();

        return new SpinResponse<IReadOnlyList<LedgerItem>>(list);
    }

    public async Task<SpinResponse> Validate(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        Option<SoftBankAccount> contract = await ReadContract(context);
        if (contract.IsError()) return contract.ToSpinResponse();

        Option signResult = await contract.Return().ValidateBlockChain(_signValidate, context);
        return signResult.ToSpinResponse();
    }

    private async Task<Option<SoftBankAccount>> ReadContract(ScopeContext context)
    {
        context.Location().LogInformation("Reading BLOB for SoftBankAccount, actorKey={actorKey}", this.GetPrimaryKeyString());
        await _state.ReadStateAsync();

        Option<SoftBankAccount> contract = _state.State.ToSoftBankAccount(_actorContext).LogResult(_actorContext.Location());
        if (contract.IsError())
        {
            context.Location().LogError("Failed to read/find BLOB for SoftBankAccount, actorKey={actorKey}", this.GetPrimaryKeyString());
            return contract;
        }

        Option signResult = await contract.Return().ValidateBlockChain(_signValidate, context);
        if (signResult.StatusCode.IsError())
        {
            context.Location().LogCritical("Contract actorId={actorId} could not be validated before writing to storage", this.GetPrimaryKeyString());
            throw new InvalidOperationException($"Contract should not be validated, actorId={this.GetPrimaryKeyString()}");
        }

        return contract;
    }

    private async Task<SpinResponse> WriteContract(SoftBankAccount contract, ScopeContext context)
    {
        context.Location().LogInformation("Writing SoftBank acocunt");

        Option signResult = await contract.ValidateBlockChain(_signValidate, context);
        if (signResult.StatusCode.IsError())
        {
            context.Location().LogCritical("Contract actorId={actorId} could not be validated before writing to storage", this.GetPrimaryKeyString());
            throw new InvalidOperationException($"Contract should not be validated, actorId={this.GetPrimaryKeyString()}");
        }

        _state.State = contract.ToBlobPackage();
        await _state.WriteStateAsync();

        return new SpinResponse(StatusCode.OK);
    }

}
