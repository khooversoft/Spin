using Microsoft.Extensions.Logging;
using SoftBank.sdk;
using SoftBank.sdk.Application;
using SoftBank.sdk.Models;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.Lease;
using SpinCluster.sdk.Actors.Signature;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;


namespace SoftBank.sdk;

// ActorKey = "softbank:company3.com/accountId"
// ContractId = "contract:company3.com/softbank/accountId"
public class SoftBankActor : Grain, ISoftBankActor
{
    private readonly ILogger<SoftBankActor> _logger;
    private readonly IClusterClient _clusterClient;
    private readonly SoftBankManagement _mgmt;

    public SoftBankActor(IClusterClient clusterClient, ILogger<SoftBankActor> logger)
    {
        _logger = logger.NotNull();
        _clusterClient = clusterClient.NotNull();

        _mgmt = new SoftBankManagement(this, _logger);
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema("softbank", new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> Delete(string traceId) => 
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting SoftBank - actorId={actorId}", this.GetPrimaryKeyString());

        var result = await GetContractActor().Delete(traceId);
        return result;
    }

    public async Task<Option> Exist(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        var result = await GetContractActor().Exist(traceId);
        return result;
    }

    public async Task<Option> Create(AccountDetail detail, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Creating contract accountDetail={accountDetail}", detail);

        var v = detail.Validate().LogResult(context.Location());
        if (v.IsError()) return v;

        IContractActor contract = GetContractActor();

        var createContractRequest = new ContractCreateModel
        {
            DocumentId = GetSoftBankContractId(),
            PrincipalId = detail.OwnerId,
            BlockAccess = detail.AccessRights.ToArray(),
        };

        var createOption = await contract.Create(createContractRequest, context.TraceId);
        if (createOption.IsError()) return createOption;

        return await Append(detail, detail.OwnerId, context);
    }

    public async Task<Option> SetAccountDetail(AccountDetail detail, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set account detail={accountDetail}", detail);

        var v = detail.Validate().LogResult(context.Location());
        if (v.IsError()) return v;

        return await Append(detail, detail.OwnerId, context);
    }

    public async Task<Option> SetAcl(BlockAcl blockAcl, string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Add BlockAcl={blockAcl}", blockAcl);

        var v = blockAcl.Validate().LogResult(context.Location());
        if (v.IsError()) return v;

        return await Append(blockAcl, principalId, context);
    }

    public async Task<Option> AddLedgerItem(LedgerItem ledgerItem, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Add Ledger item ledgerItem={ledgerItem}", ledgerItem);

        var v = ledgerItem.Validate().LogResult(context.Location());
        if (v.IsError()) return v;

        return await Append(ledgerItem, ledgerItem.OwnerId, context);
    }

    public async Task<Option<AccountDetail>> GetAccountDetail(string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Getting account detail, principalId={principalId}", principalId);
        if (!IdPatterns.IsPrincipalId(principalId)) return StatusCode.BadRequest;

        IContractActor contract = GetContractActor();

        var query = new ContractQuery
        {
            PrincipalId = principalId,
            BlockType = typeof(AccountDetail).GetTypeName(),
            LatestOnly = true,
        };

        Option<IReadOnlyList<DataBlock>> queryOption = await contract.Query(query, context.TraceId);
        if (queryOption.IsError()) return queryOption.ToOptionStatus<AccountDetail>();

        IReadOnlyList<AccountDetail> list = queryOption.Return()
            .Select(x => x.ToObject<AccountDetail>())
            .ToArray();

        if (list.Count != 1) return StatusCode.NotFound;

        AccountDetail accountDetail = list.First();
        return accountDetail;
    }

    public async Task<Option<IReadOnlyList<LedgerItem>>> GetLedgerItems(string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Getting leger items, principalId={principalId}", principalId);
        if (!IdPatterns.IsPrincipalId(principalId)) return StatusCode.BadRequest;

        IContractActor contract = GetContractActor();

        var query = new ContractQuery
        {
            PrincipalId = principalId,
            BlockType = typeof(LedgerItem).GetTypeName(),
        };

        Option<IReadOnlyList<DataBlock>> queryOption = await contract.Query(query, context.TraceId);
        if (queryOption.IsError()) return queryOption.ToOptionStatus<IReadOnlyList<LedgerItem>>();

        IReadOnlyList<LedgerItem> list = queryOption.Return()
            .Select(x => x.ToObject<LedgerItem>())
            .ToArray();

        return list.ToOption();
    }

    public async Task<Option<AccountBalance>> GetBalance(string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Getting leger balance, principalId={principalId}", principalId);
        if (!IdPatterns.IsPrincipalId(principalId)) return StatusCode.BadRequest;

        var listOption = await GetLedgerItems(principalId, traceId);
        if (listOption.IsError()) return listOption.ToOptionStatus<AccountBalance>();

        decimal balance = listOption.Return().Sum(x => x.GetNaturalAmount());

        var response = new AccountBalance
        {
            DocumentId = this.GetPrimaryKeyString(),
            Balance = balance,
        };

        return response;
    }

    public async Task<Option<AmountReserved>> Reserve(string principalId, decimal amount, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Reserve funds for trx, principalId={principalId}", principalId);

        var test = new Option()
            .Test(() => IdPatterns.IsPrincipalId(principalId))
            .Test(() => amount > 0 ? StatusCode.OK : new Option(StatusCode.BadRequest, "Amount must be positive"));
        if (test.IsError()) return test.ToOptionStatus<AmountReserved>();

        IContractActor contract = GetContractActor();
        var isOwner = await contract.HasAccess(principalId, BlockGrant.Owner, traceId);
        if (isOwner.IsError()) return isOwner.ToOptionStatus<AmountReserved>();

        // Verify money is available
        AccountBalance balance = await GetBalance(principalId, traceId).Return();
        if (balance.Balance < amount) return new Option<AmountReserved>(StatusCode.BadRequest, "No funds");

        ILeaseActor leaseActor = GetLeaseActor();

        var request = CreateLeaseRequest(amount);
        var acquire = await leaseActor.Acquire(request, context.TraceId);
        if( acquire.IsError()) return acquire.ToOptionStatus<AmountReserved>();

        var reserved = new AmountReserved
        {
            LeaseKey = request.LeaseKey,
            AccountId = this.GetPrimaryKeyString(),
            PrincipalId = principalId,
            Amount = amount,
            GoodTo = DateTime.UtcNow + request.TimeToLive,
        };

        return reserved;
    }

    public async Task<Option> ReleaseReserve(string leaseKey, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Release reserve funds for trx, leaseKey={leaseKey}", leaseKey);
        
        ILeaseActor leaseActor = GetLeaseActor();
        var releaseResponse = await leaseActor.Release(leaseKey, context.TraceId);

        return releaseResponse;
    }

    internal ResourceId GetSoftBankContractId() => this.GetPrimaryKeyString().ToSoftBankContractId();

    internal IContractActor GetContractActor() => _clusterClient.GetContractActor(GetSoftBankContractId());

    private ILeaseActor GetLeaseActor() => ResourceId.Create(this.GetPrimaryKeyString()).ThrowOnError()
        .Bind(x => IdTool.CreateLeaseId(x.Domain.NotNull(), "softbank/" + x.Path.NotNull()))
        .Bind(x => _clusterClient.GetLeaseActor(x))
        .Return();

    internal async Task<Option> Append<T>(T value, string principalId, ScopeContext context) where T : class
    {
        IContractActor contract = GetContractActor();
        ISignatureActor signatureActor = _clusterClient.GetSignatureActor();

        var dataBlock = await value.ToDataBlock(principalId).Sign(signatureActor, context);
        if (dataBlock.IsError()) return dataBlock.ToOptionStatus();

        var appendResult = await contract.Append(dataBlock.Return(), context.TraceId);
        if (appendResult.IsError()) return appendResult;

        return StatusCode.OK;
    }

    private sealed record ReserveLease
    {
        public static string BuildLeaseKey(decimal amount) => $"ReserveAmount/{Guid.NewGuid()}";
        public decimal Amount { get; init; }
    }

    private static LeaseCreate CreateLeaseRequest(decimal amount) => new LeaseCreate
    {
        LeaseKey = ReserveLease.BuildLeaseKey(amount),
        Payload = new ReserveLease { Amount = amount }.ToJson(),
    };
}
