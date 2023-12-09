using LoanContract.sdk.Models;
using Microsoft.Extensions.Logging;
using SoftBank.sdk.Application;
using SoftBank.sdk.Models;
using SoftBank.sdk.Trx;
using SpinClient.sdk;
using SpinCluster.abstraction;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Finance.Finance;
using Toolbox.Tools;
using Toolbox.Types;

namespace LoanContract.sdk.Contract;

public class LoanContractManager
{
    private readonly ContractClient _contractClient;
    private readonly SignatureClient _signatureClient;
    private readonly SoftBankTrxClient _softBankTrxClient;
    private readonly ILogger<LoanContractManager> _logger;

    public LoanContractManager(ContractClient contractClient, SignatureClient signatureClient, SoftBankTrxClient softBankTrxClient, ILogger<LoanContractManager> logger)
    {
        _contractClient = contractClient.NotNull();
        _signatureClient = signatureClient.NotNull();
        _softBankTrxClient = softBankTrxClient.NotNull();
        _logger = logger.NotNull();
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
            BlockAccess = detail.Access,
            RoleRights = detail.RoleAccess.ToArray(),
        };

        var createOption = await _contractClient.Create(createContractRequest, context);
        if (createOption.IsError()) return createOption;

        return await Append(detail, detail.ContractId, detail.OwnerId, context);
    }

    public async Task<Option<LoanDetail>> GetLoanDetail(string contractId, string principalId, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogInformation("Get loan detail, contractId={contractId}", contractId);

        var query = new ContractQueryBuilder().SetPrincipalId(principalId).Add<LoanDetail>(true).Build();

        Option<ContractQueryResponse> data = await _contractClient.Query(contractId, query, context);
        if (data.IsError()) return data.ToOptionStatus<LoanDetail>();

        var loanDetailOption = data.Return().GetSingle<LoanDetail>();
        if (loanDetailOption.IsError()) return loanDetailOption;

        return loanDetailOption;
    }

    public async Task<Option<LoanReportModel>> GetReport(string contractId, string principalId, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogInformation("Get loan report, contractId={contractId}", contractId);

        var query = new ContractQueryBuilder().SetPrincipalId(principalId).Add<LoanDetail>(true).Add<LoanLedgerItem>(false).Build();

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

    public async Task<Option> MakePayment(LoanPaymentRequest model, ScopeContext context)
    {
        if (!model.Validate(out var v)) return v;
        context = context.With(_logger);
        context.Location().LogInformation("Posting payment, contractId={contractId}, postedDate={postedDate}", model.ContractId, model.PostedDate);

        var loanDetailOption = await GetLoanDetail(model.ContractId, model.PrincipalId, context);
        if (loanDetailOption.IsError()) return loanDetailOption.ToOptionStatus();
        LoanDetail report = loanDetailOption.Return();

        var trxRequest = new TrxRequest
        {
            PrincipalId = model.PrincipalId,
            AccountID = report.OwnerSoftBankId,
            PartyAccountId = report.PartySoftBankId,
            Description = "Manager payment",
            Type = TrxType.Pull,
            Amount = model.PaymentAmount,
            Tags = model.Tags,
        };

        trxRequest.Validate().ThrowOnError("TrxRequest not valid");

        Option<TrxResponse> trxResponse = await _softBankTrxClient.Request(trxRequest, context);
        if (trxResponse.IsError())
        {
            context.Location().LogStatus(trxResponse.ToOptionStatus(), "Failed to make payment, model={model}", model);
            return trxResponse.ToOptionStatus();
        }

        return await PostPayment(model, context);
    }

    public async Task<Option> PostInterestCharge(LoanInterestRequest model, ScopeContext context)
    {
        if (!model.Validate(out var v)) return v;
        context = context.With(_logger);
        context.Location().LogInformation("Calculating interest, model={model}", model);

        var reportOption = await GetReport(model.ContractId, model.PrincipalId, context);
        if (reportOption.IsError()) return reportOption.ToOptionStatus();
        LoanReportModel report = reportOption.Return();

        int numberOfDays = report.NumberOfDays(LoanTrxType.InterestCharge, model.PostedDate);
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
            PostedDate = model.PostedDate,
            ContractId = model.ContractId,
            OwnerId = model.PrincipalId,
            Description = "Interest charge",
            Type = LoanLedgerType.Debit,
            TrxType = LoanTrxType.InterestCharge,
            Amount = interestCharge,
        };

        ledgerItem.Validate().ThrowOnError("Invalid loan ledger");

        return await AddLedgerItem(ledgerItem, context);
    }

    private async Task<Option> PostPayment(LoanPaymentRequest model, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogInformation("Posting payment, model={model}", model);

        var ledgerItem = new LoanLedgerItem
        {
            PostedDate = model.PostedDate,
            ContractId = model.ContractId,
            OwnerId = model.PrincipalId,
            Description = "Payment",
            Type = LoanLedgerType.Credit,
            TrxType = LoanTrxType.Payment,
            Amount = model.PaymentAmount,
            Tags = model.Tags,
        };

        ledgerItem.Validate().ThrowOnError("Invalid loan ledger");

        var addOption = await AddLedgerItem(ledgerItem, context);
        if (addOption.IsError())
        {
            context.Location().LogError("Failed to post payment, contractId={contractId}, postedDate={postedDate}",
                model.ContractId, model.PostedDate);
            return addOption;
        }

        context.Location().LogInformation("Posted payment, contractId={contractId}, postedDate={postedDate}",
            model.ContractId, model.PostedDate);
        return StatusCode.OK;
    }

    public async Task<Option> SetLoanDetail(LoanDetail loanDetail, ScopeContext context)
    {
        context = context.With(_logger);
        if (!loanDetail.Validate(out Option v)) return v;

        context.Location().LogInformation("Set loan detail, contractId={contractId}", loanDetail);

        return await Append(loanDetail, loanDetail.ContractId, loanDetail.OwnerId, context);
    }

    public async Task<Option> AddLedgerItem(LoanLedgerItem ledgerItem, ScopeContext context)
    {
        context = context.With(_logger);
        if (!ledgerItem.Validate(out var v)) return v;
        context.Location().LogInformation("Adding ledger, contractId={contractId}, ledgerItem={ledgerItem}", ledgerItem.ContractId, ledgerItem);

        return await Append(ledgerItem, ledgerItem.ContractId, SoftBankConstants.SoftBankPrincipalId, context);
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
