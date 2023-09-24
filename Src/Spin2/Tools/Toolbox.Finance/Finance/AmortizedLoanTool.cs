namespace Toolbox.Finance.Finance;


/// <summary>
/// Calculate different values for amortized loan type
/// </summary>
public static class AmortizedLoanTool
{
    public static decimal CalculatePayment(LoanTerms terms)
    {
        double interestRate = terms.APR / terms.PaymentsPerYear;
        double d1 = Math.Pow(1 + interestRate, terms.NumberPayments) - 1;
        double d2 = interestRate * Math.Pow(1 + interestRate, terms.NumberPayments);
        double discountFactor = d1 / d2;
        decimal monthlyPayment = (decimal)((double)terms.PrincipalAmount / discountFactor);

        return Math.Round(monthlyPayment, 2, MidpointRounding.AwayFromZero);
    }

    public static decimal CalculateInterestCharge(InterestChargeDetail detail)
    {
        // Calculate the interest charge for a specific number of days
        double dailyInterestRate = detail.APR / 365; // Convert annual interest rate to daily rate
        decimal interestCharge = (decimal)((double)detail.Principal * (dailyInterestRate * detail.NumberOfDays));

        return Math.Round(interestCharge, 2, MidpointRounding.AwayFromZero);
    }
}
