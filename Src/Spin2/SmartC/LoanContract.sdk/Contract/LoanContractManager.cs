using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public async Task<Option> CalculateInterest(string contractId, string principalId, DateTime postedDate, ScopeContext context)
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
            NumberOfDays = (int)Math.Round((postedDate - report.LedgerItems.Max(x => x.Timestamp)).TotalDays, 0, MidpointRounding.AwayFromZero),
        };

        decimal interestCharge = AmortizedLoanTool.CalculateInterestCharge(detail);

        var ledgerItem = new LoanLedgerItem
        {
            ContractId = contractId,
            OwnerId = principalId,
            Description = "Interest charge",
            Type = LoanLedgerType.Debit,
            TrxType = LoanTrxType.InterestCharge,
            Amount = interestCharge,
        };

        return await AddLedgerItem(ledgerItem, context);
    }

    public async Task<Option> Create(LoanAccountDetail detail, ScopeContext context)
    {
        context = context.With(_logger);
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

        var loanDetailOption = await GetLoanDetail(contractId, principalId, context);
        if (loanDetailOption.IsError()) return loanDetailOption.ToOptionStatus<LoanReportModel>();

        var ledgerItemsOption = await GetLedgerItems(contractId, principalId, context);
        if (ledgerItemsOption.IsError()) return ledgerItemsOption.ToOptionStatus<LoanReportModel>();

        var result = new LoanReportModel
        {
            ContractId = contractId,
            LoanDetail = loanDetailOption.Return(),
            LedgerItems = ledgerItemsOption.Return(),
        };

        return result;
    }

    public async Task<Option> SetLoanDetail(LoanDetail loanDetail, ScopeContext context)
    {
        context = context.With(_logger);
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

    private async Task<Option<LoanDetail>> GetLoanDetail(string contractId, string principalId, ScopeContext context)
    {
        var query = ContractQuery.CreateQuery<LoanDetail>(principalId, true);

        Option<IReadOnlyList<DataBlock>> loanDetail = await _contractClient.Query(contractId, query, context);
        if (loanDetail.IsError()) return loanDetail.ToOptionStatus<LoanDetail>();
        loanDetail.Return().Assert(x => x.Count == 1, x => "return row count is invalid");

        LoanDetail model = loanDetail.Return().First().ToObject<LoanDetail>();

        var v = model.Validate();
        if (v.IsError()) return v.ToOptionStatus<LoanDetail>();

        return model;
    }

    private async Task<Option<IReadOnlyList<LoanLedgerItem>>> GetLedgerItems(string contractId, string principalId, ScopeContext context)
    {
        var query = ContractQuery.CreateQuery<LoanLedgerItem>(principalId, false);

        Option<IReadOnlyList<DataBlock>> data = await _contractClient.Query(contractId, query, context);
        if (data.IsError()) return data.ToOptionStatus<IReadOnlyList<LoanLedgerItem>>();

        var ledgerItems = data.Return()
            .Select(x => x.ToObject<LoanLedgerItem>())
            .ToArray();

        var v = ledgerItems
            .Select(x => x.Validate())
            .SkipWhile(x => x.IsOk())
            .FirstOrDefault(StatusCode.OK);

        if (v.IsError()) return v.ToOptionStatus<IReadOnlyList<LoanLedgerItem>>();

        return ledgerItems;
    }
}
