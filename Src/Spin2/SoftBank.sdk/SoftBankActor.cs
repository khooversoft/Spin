using Microsoft.AspNetCore.Authentication;
using Microsoft.Azure.Amqp.Framing;
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
    private readonly SoftBankFactory _softBankFactory;
    private ScopeContext _actorContext;

    public SoftBankActor(
        [PersistentState(stateName: SpinConstants.Extension.SmartContract, storageName: SpinConstants.SpinStateStore)] IPersistentState<BlobPackage> state,
        IValidator<AccountDetail> accountValidator,
        IValidator<LedgerItem> ledgerItemValidator,
        ISign sign,
        ISignValidate signValidate,
        SoftBankFactory softBankFactory,
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
        _softBankFactory = softBankFactory.NotNull();
    }

    public async Task<Option> Delete(string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting SoftBank - actorId={actorId}", this.GetPrimaryKeyString());

        Option<SoftBankAccount> softBank = await ReadContract(context);
        if (softBank.IsError()) return softBank.ToOptionStatus();

        Option isOwner = softBank.Return().IsOwner(principalId);
        if (isOwner.StatusCode.IsError()) return isOwner;

        await _state.Delete(this.GetPrimaryKeyString(), new ScopeContext(traceId, _logger).Location());
        return StatusCode.OK;
    }

    public Task<Option> Exist(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        StatusCode exist = _state.RecordExists ? StatusCode.OK : StatusCode.NoContent;
        context.Location().LogInformation("Exist, exist={exist}, actorId={actorId}", exist, this.GetPrimaryKeyString());

        return new Option(exist).ToTaskResult();
    }

    public async Task<Option> Create(AccountDetail detail, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Creating contract accountDetail={accountDetail}", detail);
        if (!detail.IsValid(context.Location())) return new Option(StatusCode.BadRequest);

        if (_state.RecordExists)
        {
            context.Location().LogInformation("Cannot create contract, already exist, actorId={actorId}", this.GetPrimaryKeyString());
            return new Option(StatusCode.BadRequest, $"Cannot create contract, already exist, actorId={this.GetPrimaryKeyString()}");
        }

        var acl = new BlockAcl(detail.AccessRights);
        var softBank = await _softBankFactory.Create(detail.ObjectId.ToObjectId(), detail.OwnerId, acl, context).Return();

        Option writeResult = await softBank.AccountDetail.Set(detail, context);
        if (writeResult.StatusCode.IsError()) return writeResult;

        return await WriteContract(softBank, context);
    }

    public async Task<Option> SetAccountDetail(AccountDetail detail, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set account detail={accountDetail}", detail);

        Option<SoftBankAccount> softBank = await ReadContract(context);
        if (softBank.IsError()) return softBank.ToOptionStatus();

        Option result = await softBank.Return().AccountDetail.Set(detail, context);
        if (result.StatusCode.IsError()) return result;

        return await WriteContract(softBank.Return(), context);
    }

    public async Task<Option> SetAcl(BlockAcl blockAcl, string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Add BlockAcl={blockAcl}", blockAcl);

        Option<SoftBankAccount> softBank = await ReadContract(context);
        if (softBank.IsError()) return softBank.ToOptionStatus();

        Option result = await softBank.Return().Acl.Set(blockAcl, principalId, context);
        if (result.StatusCode.IsError()) return result;

        return await WriteContract(softBank.Return(), context);
    }

    public async Task<Option> AddLedgerItem(LedgerItem ledgerItem, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        Option<SoftBankAccount> softBank = await ReadContract(context);
        if (softBank.IsError()) return softBank.ToOptionStatus();

        Option result = await softBank.Return().LedgerItems.Add(ledgerItem, context);
        if (result.StatusCode.IsError()) return result;

        return await WriteContract(softBank.Return(), context);
    }

    public async Task<Option<AccountDetail>> GetAccountDetail(string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        Option<SoftBankAccount> softBank = await ReadContract(context);
        if (softBank.IsError()) return softBank.ToOptionStatus<AccountDetail>();

        Option<AccountDetail> accountDetail = softBank.Return().AccountDetail.Get(principalId, context);
        if (accountDetail.IsError()) return accountDetail;

        return new Option<AccountDetail>(accountDetail.Return());
    }

    public async Task<Option<AccountBalance>> GetBalance(string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        Option<SoftBankAccount> softBankAccount = await ReadContract(context);
        if (softBankAccount.IsError()) return softBankAccount.ToOptionStatus<AccountBalance>();

        Option<decimal> balance = softBankAccount.Return().LedgerItems.GetBalance(principalId, context);
        if (balance.IsError()) return balance.ToOptionStatus<AccountBalance>();

        Option<AccountDetail> detailOption = softBankAccount.Return().AccountDetail.Get(principalId, context);
        if (detailOption.IsError()) return detailOption.ToOptionStatus<AccountBalance>();

        AccountDetail detail = detailOption.Return();

        var result = new AccountBalance
        {
            ObjectId = detail.ObjectId,
            OwnerId = detail.OwnerId,
            Name = detail.Name,
            CreatedDate = detail.CreatedDate,
            Balance = balance.Return(),
        };

        return new Option<AccountBalance>(result);
    }

    public async Task<Option<IReadOnlyList<LedgerItem>>> GetLedgerItems(string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        Option<SoftBankAccount> softBank = await ReadContract(context);
        if (softBank.IsError()) return softBank.ToOptionStatus<IReadOnlyList<LedgerItem>>();

        Option<BlockReader<LedgerItem>> stream = softBank.Return().LedgerItems.GetReader(principalId, context);
        if (stream.IsError()) return stream.ToOptionStatus<IReadOnlyList<LedgerItem>>();

        IReadOnlyList<LedgerItem> list = stream.Return().List();
        return new Option<IReadOnlyList<LedgerItem>>(list);
    }

    public async Task<Option> Validate(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        Option<SoftBankAccount> contract = await ReadContract(context);
        if (contract.IsError()) return contract.ToOptionStatus();

        Option signResult = await contract.Return().ValidateBlockChain(context);
        return signResult;
    }

    private async Task<Option<SoftBankAccount>> ReadContract(ScopeContext context)
    {
        context.Location().LogInformation("Reading BLOB for SoftBankAccount, actorKey={actorKey}", this.GetPrimaryKeyString());
        await _state.ReadStateAsync();
        if (!_state.RecordExists) return new Option<SoftBankAccount>(StatusCode.NotFound);

        Option<SoftBankAccount> contract = await _softBankFactory.Create(_state.State, context).LogResult(_actorContext.Location());
        if (contract.IsError())
        {
            context.Location().LogError("Failed to read/find BLOB for SoftBankAccount, actorKey={actorKey}", this.GetPrimaryKeyString());
            return contract;
        }

        Option signResult = await contract.Return().ValidateBlockChain(context);
        if (signResult.StatusCode.IsError())
        {
            context.Location().LogCritical("Contract actorId={actorId} could not be validated before writing to storage", this.GetPrimaryKeyString());
            throw new InvalidOperationException($"Contract should not be validated, actorId={this.GetPrimaryKeyString()}");
        }

        return contract;
    }

    private async Task<Option> WriteContract(SoftBankAccount contract, ScopeContext context)
    {
        context.Location().LogInformation("Writing SoftBank acocunt");

        Option signResult = await contract.ValidateBlockChain(context);
        if (signResult.StatusCode.IsError())
        {
            context.Location().LogCritical("Contract actorId={actorId} could not be validated before writing to storage", this.GetPrimaryKeyString());
            throw new InvalidOperationException($"Contract should not be validated, actorId={this.GetPrimaryKeyString()}");
        }

        _state.State = contract.ToBlobPackage();
        await _state.WriteStateAsync();

        return new Option(StatusCode.OK);
    }
}
