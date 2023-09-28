using LoanContract.sdk.Models;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Actors.Signature;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Finance.Finance;
using Toolbox.Tools;
using Toolbox.Types;

namespace LoanContract.sdk.Contract;

public class LoanContractManager
{
    private readonly ContractClient _contractClient;
    private readonly ILogger<LoanContractManager> _logger;
    private readonly SignatureClient _signatureClient;

    public LoanContractManager(ContractClient contractClient, SignatureClient signatureClient, ILogger<LoanContractManager> logger)
    {
        _contractClient = contractClient.NotNull();
        _signatureClient = signatureClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> AddLedgerItem(LoanLedgerItem ledgerItem, ScopeContext context)
    {
        context = context.With(_logger);
        if (!ledgerItem.Validate(out var v)) return v;

        context.Location().LogInformation("Adding ledger, contractId={contractId}, ledgerItem={ledgerItem}", ledgerItem.ContractId, ledgerItem);

        return await Append(ledgerItem, ledgerItem.ContractId, ledgerItem.OwnerId, context);
    }

    public async Task<Option> PostInterestCharge(string contractId, string principalId, DateTime postedDate, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogInformation("Calculating interest, contractId={contractId}", contractId);

        var reportOption = await GetReport(contractId, principalId, context);
        if (reportOption.IsError()) return reportOption.ToOptionStatus();

        LoanReportModel report = reportOption.Return();

        var detail = new InterestChargeDetail
        {
            Principal = report.GetPrincipalAmount(),
            APR = report.LoanDetail.APR,
            NumberOfDays = (int)Math.Round(calculateNumberOfDays(report), 0, MidpointRounding.AwayFromZero),
        };

        decimal interestCharge = AmortizedLoanTool.CalculateInterestCharge(detail);

        var ledgerItem = new LoanLedgerItem
        {
            Timestamp = postedDate,
            ContractId = contractId,
            OwnerId = principalId,
            Description = "Interest charge",
            Type = LoanLedgerType.Debit,
            TrxType = LoanTrxType.InterestCharge,
            Amount = interestCharge,
        };

        return await AddLedgerItem(ledgerItem, context);

        double calculateNumberOfDays(LoanReportModel report) => report.LedgerItems switch
        {
            { Count: 0 } => (postedDate - report.LoanDetail.FirstPaymentDate).TotalDays,
            var v => (postedDate - v.Max(x => x.Timestamp)).TotalDays,
        };
    }

    public async Task<Option> Create(LoanAccountDetail detail, ScopeContext context)
    {
        context = context.With(_logger);
        if (!detail.Validate(out Option v)) return v;
        context.Location().LogInformation("Calculating interest, contractId={contractId}", detail.ContractId);

        var createContractRequest = new ContractCreateModel
        {
            DocumentId = detail.ContractId,
            PrincipalId = detail.OwnerId,
            BlockAccess = detail.AccessRights.ToArray(),
            RoleRights = detail.RoleRights.ToArray(),
        };

        var createOption = await _contractClient.Create(createContractRequest, context);
        if (createOption.IsError()) return createOption;

        return await Append(detail, detail.ContractId, detail.OwnerId, context);
    }

    public async Task<Option> Delete(string contractId, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogInformation("Calculating interest, contractId={contractId}", contractId);

        var option = await _contractClient.Delete(contractId, context);
        return option;
    }

    public async Task<Option<LoanReportModel>> GetReport(string contractId, string principalId, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogInformation("Calculating interest, contractId={contractId}", contractId);

        var query = new ContractQuery
        {
            PrincipalId = principalId,
            BlockTypes = new[]
            {
                new QueryBlockType { BlockType = typeof(LoanDetail).GetTypeName(), LatestOnly = true },
                new QueryBlockType { BlockType = typeof(LoanLedgerItem).GetTypeName(), LatestOnly = false },
            }.ToArray(),
        };

        Option<ContractQueryResponse> data = await _contractClient.Query(contractId, query, context);
        if (data.IsError()) return data.ToOptionStatus<LoanReportModel>();

        var loanDetailOption = data.Return().GetSingle<LoanDetail>();
        if (loanDetailOption.IsError()) return loanDetailOption.ToOptionStatus<LoanReportModel>();

        IReadOnlyList<LoanLedgerItem> ledgerItems = data.Return().GetItems<LoanLedgerItem>();

        //var loanDetailOption = await GetLoanDetail(contractId, principalId, context);
        //if (loanDetailOption.IsError()) return loanDetailOption.ToOptionStatus<LoanReportModel>();

        //var ledgerItemsOption = await GetLedgerItems(contractId, principalId, context);
        //if (ledgerItemsOption.IsError()) return ledgerItemsOption.ToOptionStatus<LoanReportModel>();

        var result = new LoanReportModel
        {
            ContractId = contractId,
            LoanDetail = loanDetailOption.Return(),
            LedgerItems = ledgerItems,
        };

        return result;
    }

    public async Task<Option> SetLoanDetail(LoanDetail loanDetail, ScopeContext context)
    {
        context = context.With(_logger);
        if (!loanDetail.Validate(out Option v)) return v;

        context.Location().LogInformation("Set loan detail, contractId={contractId}", loanDetail);

        return await Append(loanDetail, loanDetail.ContractId, loanDetail.OwnerId, context);
    }

    private async Task<Option> Append<T>(T value, string contractId, string principalId, ScopeContext context) where T : class
    {
        var dataBlock = await value.ToDataBlock(principalId).Sign(_signatureClient, context);
        if (dataBlock.IsError()) return dataBlock.ToOptionStatus();

        var appendResult = await _contractClient.Append(contractId, dataBlock.Return(), context);
        if (appendResult.IsError()) return appendResult;

        return StatusCode.OK;
    }
}
