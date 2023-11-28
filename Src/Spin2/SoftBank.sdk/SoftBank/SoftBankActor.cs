using Microsoft.Extensions.Logging;
using SoftBank.sdk.Application;
using SoftBank.sdk.Models;
using SpinCluster.abstraction;
using SpinCluster.sdk.Actors;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.Signature;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;


namespace SoftBank.sdk.SoftBank;

public interface ISoftBankActor : IGrainWithStringKey
{
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option> Create(SbAccountDetail detail, string traceId);

    Task<Option> SetAccountDetail(SbAccountDetail detail, string traceId);
    Task<Option> SetAcl(AclBlock blockAcl, string principalId, string traceId);

    Task<Option> AddLedgerItem(SbLedgerItem ledgerItem, string traceId);
    Task<Option<SbAccountDetail>> GetAccountDetail(string principalId, string traceId);
    Task<Option<IReadOnlyList<SbLedgerItem>>> GetLedgerItems(string principalId, string traceId);
    Task<Option<SbAccountBalance>> GetBalance(string principalId, string traceId);

    Task<Option<SbAccountBalance>> GetReserveBalance(string leaseKey, string traceId);
    Task<Option<SbAmountReserved>> Reserve(string principalId, decimal amount, string traceId);
    Task<Option> ReleaseReserve(string leaseKey, string traceId);
    Task<Option<IReadOnlyList<LeaseData>>> GetActiveReservations(string principalId, string traceId);
}

// ActorKey = "softbank:company3.com/accountId"
// ContractId = "contract:company3.com/softbank/accountId"
public class SoftBankActor : Grain, ISoftBankActor
{
    private readonly ILogger<SoftBankActor> _logger;
    private readonly IClusterClient _clusterClient;
    private readonly SoftBank_Management _mgmt;
    private readonly SoftBank_AccountDetail _detail;
    private readonly SoftBank_Ledger _ledger;
    private readonly SoftBank_ReserveFund _reserve;

    public SoftBankActor(IClusterClient clusterClient, ILogger<SoftBankActor> logger)
    {
        _logger = logger.NotNull();
        _clusterClient = clusterClient.NotNull();

        _mgmt = new SoftBank_Management(this, _logger);
        _detail = new SoftBank_AccountDetail(this, _logger);
        _ledger = new SoftBank_Ledger(this, _logger);
        _reserve = new SoftBank_ReserveFund(this, _clusterClient, _logger);
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema("softbank", new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }

    public Task<Option> Delete(string traceId) => _mgmt.Delete(traceId);
    public Task<Option> Exist(string traceId) => _mgmt.Exist(traceId);
    public Task<Option> Create(SbAccountDetail detail, string traceId) => _mgmt.Create(detail, traceId);
    public Task<Option> SetAcl(AclBlock blockAcl, string principalId, string traceId) => _mgmt.SetAcl(blockAcl, principalId, traceId);

    public Task<Option> SetAccountDetail(SbAccountDetail detail, string traceId) => _detail.SetAccountDetail(detail, traceId);
    public Task<Option<SbAccountDetail>> GetAccountDetail(string principalId, string traceId) => _detail.GetAccountDetail(principalId, traceId);

    public Task<Option> AddLedgerItem(SbLedgerItem ledgerItem, string traceId) => _ledger.AddLedgerItem(ledgerItem, traceId);
    public Task<Option<IReadOnlyList<SbLedgerItem>>> GetLedgerItems(string principalId, string traceId) => _ledger.GetLedgerItems(principalId, traceId);
    public Task<Option<SbAccountBalance>> GetBalance(string principalId, string traceId) => _ledger.GetBalance(principalId, traceId);

    public Task<Option<SbAmountReserved>> Reserve(string principalId, decimal amount, string traceId) => _reserve.Reserve(principalId, amount, traceId);
    public Task<Option> ReleaseReserve(string leaseKey, string traceId) => _reserve.ReleaseReserve(leaseKey, traceId);
    public Task<Option<SbAccountBalance>> GetReserveBalance(string principalId, string traceId) => _reserve.GetReserveBalance(principalId, traceId);
    public Task<Option<IReadOnlyList<LeaseData>>> GetActiveReservations(string principalId, string traceId) => _reserve.GetActiveReservations(principalId, traceId);

    internal ResourceId GetSoftBankContractId() => this.GetPrimaryKeyString().ToSoftBankContractId();
    internal IContractActor GetContractActor() => _clusterClient.GetResourceGrain<IContractActor>(GetSoftBankContractId());

    internal async Task<Option> Append<T>(T value, string principalId, ScopeContext context) where T : class
    {
        IContractActor contract = GetContractActor();
        ISignatureActor signatureActor = _clusterClient.GetResourceGrain<ISignatureActor>(SpinConstants.SignValidation);

        var dataBlock = await value.ToDataBlock(principalId).Sign(signatureActor, context);
        if (dataBlock.IsError()) return dataBlock.ToOptionStatus();

        var appendResult = await contract.Append(dataBlock.Return(), context.TraceId);
        if (appendResult.IsError()) return appendResult;

        return StatusCode.OK;
    }
}
