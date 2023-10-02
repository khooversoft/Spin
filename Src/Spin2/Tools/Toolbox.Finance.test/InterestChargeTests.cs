using FluentAssertions;
using Toolbox.Finance.Finance;

namespace Toolbox.Finance.test;

public class InterestChargeTests
{
    [Fact]
    public void InterestChargeCalculate()
    {
        var detail = new InterestChargeDetail
        {
            Principal = 100000,
            APR = 0.05,
            NumberOfDays = 30,
        };

        decimal interestCharge = AmortizedLoanTool.CalculateInterestCharge(detail);
        interestCharge.Should().Be(410.96m);
    }

    [Fact]
    public void TestPayments()
    {
        var terms = new LoanTerms
        {
            PrincipalAmount = 10_000,
            APR = 0.05,
            NumberPayments = 12,
            PaymentsPerYear = 12,
        };

        decimal payment = AmortizedLoanTool.CalculatePayment(terms);
        payment.Should().Be(856.07m);

        var list = new List<(decimal principal, decimal toPrincipal, decimal interestCharge)>();
        decimal principal = 10_000.00m;

        for (int i = 0; i < 12; i++)
        {
            var detail = new InterestChargeDetail
            {
                Principal = principal,
                APR = terms.APR,
                NumberOfDays = DateTime.DaysInMonth(2023, i + 1),
            };

            decimal interestCharge = AmortizedLoanTool.CalculateInterestCharge(detail);
            decimal toPrincipal = payment - interestCharge;
            principal -= toPrincipal;

            list.Add((principal, toPrincipal, interestCharge));
        }

        var match = new List<(decimal principal, decimal toPrincipal, decimal interestCharge)>()
        {
            (9186.40m, 813.60m, 42.47m),
            (8365.57m, 820.83m, 35.24m),
            (7545.03m, 820.54m, 35.53m),
            (6719.97m, 825.06m, 31.01m),
            (5892.44m, 827.53m, 28.54m),
            (5060.59m, 831.85m, 24.22m),
            (4226.01m, 834.58m, 21.49m),
            (3387.89m, 838.12m, 17.95m),
            (2545.74m, 842.15m, 13.92m),
            (1700.48m, 845.26m, 10.81m),
            (851.40m, 849.08m, 6.99m),
            (-1.05m, 852.45m, 3.62m),
        };

        (list.Count == match.Count).Should().BeTrue();
        Enumerable.SequenceEqual(list, match).Should().BeTrue();

        principal.Should().Be(-1.05m);
    }
}
