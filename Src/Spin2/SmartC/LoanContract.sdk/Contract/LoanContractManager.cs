using LoanContract.sdk.Models;
using Microsoft.Extensions.Logging;
using SoftBank.sdk.Models;
using SoftBank.sdk.Trx;
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
    private readonly SoftBankTrxClient _softBankTrxClient;

    public LoanContractManager(ContractClient contractClient, SignatureClient signatureClient, SoftBankTrxClient softBankTrxClient, ILogger<LoanContractManager> logger)
    {
        _contractClient = contractClient.NotNull();
        _signatureClient = signatureClient.NotNull();
        _softBankTrxClient = softBankTrxClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> AddLedgerItem(LoanLedgerItem ledgerItem, ScopeContext context)
    {
        context = context.With(_logger);
        if (!ledgerItem.Validate(out var v)) return v;

        context.Location().LogInformation("Adding ledger, contractId={contractId}, ledgerItem={ledgerItem}", ledgerItem.ContractId, ledgerItem);

        return await Append(ledgerItem, ledgerItem.ContractId, ledgerItem.OwnerId, context);
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

        var query = new ContractQueryBuilder()
            .SetPrincipalId(principalId)
            .Add<LoanDetail>(true)
            .Add<LoanLedgerItem>(false)
            .Build();

        Option<ContractQueryResponse> data = await _contractClient.Query(contractId, query, context);
        if (data.IsError()) return data.ToOptionStatus<LoanReportModel>();

        var loanDetailOption = data.Return().GetSingle<LoanDetail>();
        if (loanDetailOption.IsError()) return loanDetailOption.ToOptionStatus<LoanReportModel>();

        var result = new LoanReportModel
        {
            ContractId = contractId,
            LoanDetail = loanDetailOption.Return(),
            LedgerItems = data.Return().GetItems<LoanLedgerItem>(),
        };

        result = result with { BalanceItems = result.BuildBalance() };
        return result;
    }

    public async Task<Option> PostInterestCharge(string contractId, string principalId, DateTime postedDate, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogInformation("Calculating interest, contractId={contractId}", contractId);

        var reportOption = await GetReport(contractId, principalId, context);
        if (reportOption.IsError()) return reportOption.ToOptionStatus();
        LoanReportModel report = reportOption.Return();

        int numberOfDays = report.NumberOfDays(LoanTrxType.InterestCharge, postedDate);
        if (numberOfDays <= 0) return StatusCode.OK;

        var detail = new InterestChargeDetail
        {
            Principal = report.GetPrincipalAmount(),
            APR = report.LoanDetail.APR,
            NumberOfDays = numberOfDays,
        };

        decimal interestCharge = AmortizedLoanTool.CalculateInterestCharge(detail);

        var ledgerItem = new LoanLedgerItem
        {
            PostedDate = postedDate,
            ContractId = contractId,
            OwnerId = principalId,
            Description = "Interest charge",
            Type = LoanLedgerType.Debit,
            TrxType = LoanTrxType.InterestCharge,
            Amount = interestCharge,
        };
        ledgerItem.Validate().ThrowOnError("Invalid loan ledger");

        return await AddLedgerItem(ledgerItem, context);
    }

    public async Task<Option> MakePayment(string contractId, string principalId, DateTime postedDate, decimal paymentAmount, string reference, ScopeContext context)
    {
        paymentAmount.Assert(x => x >= 0.0m, x => $"{x} not valid");
        context = context.With(_logger);
        context.Location().LogInformation("Posting payment, contractId={contractId}, postedDate={postedDate}", contractId, postedDate);

        var interestOption = await PostInterestCharge(contractId, principalId, postedDate, context);
        if (interestOption.IsError()) return interestOption;

        var reportOption = await GetReport(contractId, principalId, context);
        if (reportOption.IsError()) return reportOption.ToOptionStatus();
        LoanReportModel report = reportOption.Return();

        var trxRequest = new TrxRequest
        {
            PrincipalId = principalId,
            AccountID = report.LoanDetail.OwnerSoftBankId,
            PartyAccountId = report.LoanDetail.PartySoftBankId,
            Description = "Manager payment",
            Type = TrxType.Pull,
            Amount = paymentAmount,
            Tags = new Tags().Set(nameof(reference), reference).ToString(),
        };
        trxRequest.Validate().ThrowOnError("TrxRequest not valid");

        Option<TrxResponse> trxResponse = await _softBankTrxClient.Request(trxRequest, context);
        if (trxResponse.IsError())
        {
            context.Location().LogError("Failed to make payment, contractId={contractId}, postedDate={postedDate}, trxResponse={trxResponse}, statusCode={statusCode}, error={error}",
                contractId, postedDate, trxResponse.Return(), trxResponse.StatusCode, trxResponse.Error);

            return trxResponse.ToOptionStatus();
        }

        return await PostPayment(contractId, principalId, postedDate, paymentAmount, reference, context);
    }

    private async Task<Option> PostPayment(string contractId, string principalId, DateTime postedDate, decimal paymentAmount, string reference, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogInformation("Posting payment, contractId={contractId}", contractId);

        var ledgerItem = new LoanLedgerItem
        {
            PostedDate = postedDate,
            ContractId = contractId,
            OwnerId = principalId,
            Description = "Payment",
            Type = LoanLedgerType.Credit,
            TrxType = LoanTrxType.Payment,
            Amount = paymentAmount,
            Tags = new Tags().Set(nameof(reference), reference).ToString(),
        };
        ledgerItem.Validate().ThrowOnError("Invalid loan ledger");

        var addOption = await AddLedgerItem(ledgerItem, context);
        if (addOption.IsError())
        {
            context.Location().LogError("Failed to post payment, contractId={contractId}, postedDate={postedDate}", contractId, postedDate);
            return addOption;
        }

        context.Location().LogInformation("Posted payment, contractId={contractId}, postedDate={postedDate}", contractId, postedDate);
        return StatusCode.OK;
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
