using Bank.Abstractions.Model;
using Toolbox.Abstractions;

namespace ContractHost.sdk;

public interface IBankServices
{
    Task<TrxBatch<TrxRequestResponse>> MoveMoney(IEnumerable<TrxRequest> requests, CancellationToken token = default);

    Task<TrxBalance?> GetBalance(DocumentId documentId, CancellationToken token = default);
}


public interface IContractService
{
    DateTimeOffset CurrentDate { get; }

    Task LoadContract();

    Task<decimal> GetBalance();

    Task<decimal> GetInterestRate();

    Task ApplyPayment(decimal principle, decimal interestCharge);
}

public interface IFinancialService
{
    public decimal CalculatePayment(int periods, decimal interestRate);

    public (decimal Principle, decimal InterestCharge) CalculateApplyPayment(decimal principle, decimal interestRate, TimeSpan period);
}
