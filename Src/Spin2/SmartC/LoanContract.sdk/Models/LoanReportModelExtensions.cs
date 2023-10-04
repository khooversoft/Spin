using Toolbox.Finance.Finance;

namespace LoanContract.sdk.Models;

public static class LoanReportModelExtensions
{
    public static int NumberOfDays(this LoanReportModel report, LoanTrxType trxType, DateTime postedDate) => report.LedgerItems switch
    {
        { Count: 0 } => (int)(postedDate - report.LoanDetail.FirstPaymentDate).TotalDays,
        var v => (int)(postedDate - v.Where(x => x.TrxType == trxType).Max(x => x.PostedDate)).TotalDays,
    };

    public static decimal GetPrincipalAmount(this LoanReportModel report) => report.LoanDetail.PrincipalAmount - report.LedgerItems
        .Where(x => x.TrxType == LoanTrxType.Payment || x.TrxType == LoanTrxType.InterestCharge)
        .Sum(x => x.NaturalAmount);

    public static decimal PaymentDue(this LoanReportModel report, DateTime postedDate)
    {
        var paymentDates = report.GetOutstandingPaymentSchedule(postedDate);
        if (paymentDates.Count == 0) return 0.0m;

        DateTime firstPayment = paymentDates.Min(x => x);

        decimal paymentDue = (paymentDates.Count * report.LoanDetail.Payment) - report.LedgerItems
            .Where(x => x.TrxType == LoanTrxType.Payment && x.PostedDate >= firstPayment)
            .Sum(x => x.NaturalAmount);

        decimal totalDue = report.GetTotalDue(postedDate);

        return Math.Max(paymentDue, totalDue);
    }

    public static decimal GetTotalDue(this LoanReportModel report, DateTime postedDate)
    {
        decimal totalDue = report.CalculatePaymentSchedule(postedDate).Count * report.LoanDetail.Payment;
        decimal totalPayed = report.LedgerItems.Where(x => x.TrxType == LoanTrxType.Payment).Sum(x => x.NaturalAmount());

        return totalDue - totalPayed;
    }

    public static IReadOnlyList<DateTime> CalculatePaymentSchedule(this LoanReportModel report, DateTime postedDate) =>
        ScheduleTool.CalculatePaymentSchedule(report.LoanDetail.FirstPaymentDate, report.LoanDetail.NumberPayments, report.LoanDetail.PaymentsPerYear)
            .Where(x => x <= postedDate)
            .OrderBy(x => x)
            .ToArray();

    public static IReadOnlyList<DateTime> GetOutstandingPaymentSchedule(this LoanReportModel report, DateTime postedDate)
    {
        IReadOnlyList<DateTime> scheduleDates = report.CalculatePaymentSchedule(postedDate);

        IReadOnlyList<DateTime> chargeDates = report.LedgerItems
            .Where(x => x.TrxType == LoanTrxType.PrincipalCharge)
            .Select(x => x.PostedDate)
            .ToArray();

        var outstanding = scheduleDates.Except(chargeDates).ToArray();
        return outstanding;
    }

    public static IReadOnlyList<LedgerBalanceItem> BuildBalance(this LoanReportModel report)
    {
        decimal principalBalance = report.LoanDetail.PrincipalAmount;
        decimal interestCharge = 0.0m;
        decimal paymentBalance = 0.0m;
        decimal totalToPrincipal = 0.0m;
        var list = new List<LedgerBalanceItem>();

        foreach (var item in report.LedgerItems)
        {
            decimal payment = item switch
            {
                { TrxType: LoanTrxType.Payment } => item.Amount,
                _ => 0,
            };
            paymentBalance += payment;

            decimal creditCharge = item switch
            {
                { TrxType: LoanTrxType.InterestCharge } => item.Amount,
                _ => 0,
            };
            interestCharge += creditCharge;

            decimal toPrincipal = item switch
            {
                { TrxType: LoanTrxType.Payment } => Math.Max(0, paymentBalance - interestCharge - totalToPrincipal),
                _ => 0,
            };

            totalToPrincipal += toPrincipal;
            principalBalance -= toPrincipal;

            var balance = new LedgerBalanceItem
            {
                PostedDate = item.PostedDate,
                CreditCharge = creditCharge,
                Payment = payment,
                ToPrincipal = toPrincipal,
                PrincipalBalance = principalBalance,
            };

            list.Add(balance);
        }

        list = list
            .GroupBy(x => x.PostedDate)
            .Select(x => new LedgerBalanceItem
            {
                PostedDate = x.Key,
                CreditCharge = x.Sum(x => x.CreditCharge),
                Payment = x.Sum(x => x.Payment),
                ToPrincipal = x.Sum(x => x.ToPrincipal),
                PrincipalBalance = x.Min(x => x.PrincipalBalance),
            })
            .ToList();

        return list;
    }
}