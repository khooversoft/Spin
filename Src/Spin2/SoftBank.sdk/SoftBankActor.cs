using Microsoft.Extensions.Logging;
using SoftBank.sdk.Application;
using SoftBank.sdk.Models;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.Signature;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Orleans.Types;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;


namespace SpinCluster.sdk.Actors.SoftBank;

public interface ISoftBankActor : IGrainWithStringKey
{
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option> Create(AccountDetail detail, string traceId);
    Task<Option> SetAccountDetail(AccountDetail detail, string traceId);
    Task<Option> SetAcl(BlockAcl blockAcl, string principalId, string traceId);
    Task<Option> AddLedgerItem(LedgerItem ledgerItem, string traceId);
    Task<Option<AccountDetail>> GetAccountDetail(string principalId, string traceId);
    Task<Option<IReadOnlyList<LedgerItem>>> GetLedgerItems(string principalId, string traceId);
    Task<Option<AccountBalance>> GetBalance(string principalId, string traceId);
}

// ActorKey = "softbank:company3.com/accountId"
// ContractId = "contract:company3.com/softbank/accountId"
public class SoftBankActor : Grain, ISoftBankActor
{
    private readonly ILogger<SoftBankActor> _logger;
    private readonly IClusterClient _clusterClient;

    public SoftBankActor(IClusterClient clusterClient, ILogger<SoftBankActor> logger)
    {
        _logger = logger.NotNull();
        _clusterClient = clusterClient;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema("softbank", new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting SoftBank - actorId={actorId}", this.GetPrimaryKeyString());

        var result = await GetContractorActor().Delete(traceId);
        return result;
    }

    public async Task<Option> Exist(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        var result = await GetContractorActor().Exist(traceId);
        return result;
    }

    public async Task<Option> Create(AccountDetail detail, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Creating contract accountDetail={accountDetail}", detail);

        var v = detail.Validate().LogResult(context.Location());
        if (v.IsError()) return v;

        IContractActor contract = GetContractorActor();

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

        IContractActor contract = GetContractorActor();

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

        IContractActor contract = GetContractorActor();

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

    private ResourceId GetSoftBankContractId() => this.GetPrimaryKeyString().ToSoftBankContractId();

    private IContractActor GetContractorActor() => _clusterClient.GetContractActor(GetSoftBankContractId());

    private async Task<Option> Append<T>(T value, string principalId, ScopeContext context) where T : class
    {
        IContractActor contract = GetContractorActor();
        ISignatureActor signatureActor = _clusterClient.GetSignatureActor();

        var dataBlock = await value.ToDataBlock(principalId).Sign(signatureActor, context);
        if (dataBlock.IsError()) return dataBlock.ToOptionStatus();

        var appendResult = await contract.Append(dataBlock.Return(), context.TraceId);
        if (appendResult.IsError()) return appendResult;

        return StatusCode.OK;
    }
}
