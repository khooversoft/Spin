using Toolbox.Finance.Finance;
using Toolbox.Tools;

namespace Toolbox.Finance.test;

public class CalculatePaymentTests
{
    [Fact]
    public void CalculatePayment_12_360_Test()
    {
        var terms = new LoanTerms
        {
            PrincipalAmount = 100000,
            APR = 0.05,
            NumberPayments = 360,
            PaymentsPerYear = 12,
        };

        decimal payment = AmortizedLoanTool.CalculatePayment(terms);
        payment.Be(536.82m);
    }

    [Fact]
    public void CalculatePayment_12_12_Test()
    {
        var terms = new LoanTerms
        {
            PrincipalAmount = 100000,
            APR = 0.06,
            NumberPayments = 12,
            PaymentsPerYear = 12,
        };

        decimal payment = AmortizedLoanTool.CalculatePayment(terms);
        payment.Be(8606.64m);
    }
}
