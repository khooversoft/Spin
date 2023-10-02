using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Finance.Finance;

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
        payment.Should().Be(536.82m);
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
        payment.Should().Be(8606.64m);
    }
}
