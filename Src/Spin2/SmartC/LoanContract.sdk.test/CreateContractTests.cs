using LoanContract.sdk.test.Application;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Types;
using SpinTestTools.sdk.ObjectBuilder;
using FluentAssertions;
using LoanContract.sdk.Models;
using Microsoft.Extensions.DependencyInjection;
using LoanContract.sdk.Contract;
using Toolbox.Finance.Finance;

namespace LoanContract.sdk.test;

public class CreateContractTests : IClassFixture<ClusterApiFixture>
{
    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    private const string _setup = """
        {
           "Configs": [
              {
                "ConfigId": "spinconfig:validDomain",
                "Configs": {
                    "outlook.com": "true",
                    "gmail.com" : "true"
                }
              }
            ],
           "Subscriptions": [
              {
                "SubscriptionId": "subscription:rentalManagment",
                "Name": "Rental Management",
                "ContactName": "user1@rental.com",
                "Email": "admin@rental.com"
              }
            ],
           "Tenants": [
              {
                "TenantId": "tenant:rental.com",
                "Subscription": "Rental tenant",
                "Domain": "rental.com",
                "SubscriptionId": "subscription:rentalManagment",
                "ContactName": "Admin",
                "Email": "admin@rental.com",
                "Enabled": true
              }        
            ],
           "Users": [
              {
                "UserId": "user:user1@rental.com",
                "PrincipalId": "user1@rental.com",
                "DisplayName": "User 1",
                "FirstName": "user1first",
                "LastName": "user1last"
              },
              {
                "UserId": "user:user1@outlook.com",
                "PrincipalId": "user1@outlook.com",
                "DisplayName": "External user 1",
                "FirstName": "user1first",
                "LastName": "user1last"
              }
            ],
           "SbAccounts": [
              {
                "AccountId": "softbank:user1@rental.com/primary",
                "OwnerId": "user1@rental.com",
                "Name": "User1"
              },
              {
                "AccountId": "softbank:user1@outlook.com/primary",
                "OwnerId": "user1@outlook.com",
                "Name": "User1 outlook.com"
              }
            ]
        }
        """;

    public CreateContractTests(ClusterApiFixture fixture)
    {
        _cluster = fixture;
    }

    [Fact]
    public async Task LifecycleTest()
    {
        var manager = _cluster.ServiceProvider.GetRequiredService<LoanContractManager>();

        var result = await new TestObjectBuilder()
            .SetJson(_setup)
            .SetService(_cluster.ServiceProvider)
            .AddStandard()
            .Build(_context);

        result.IsOk().Should().BeTrue();

        const string contractId = "contract:rental.com/loan/loanToUser1";
        const string ownerId = "user1@rental.com";

        var createRequest = new LoanAccountDetail
        {
            ContractId = contractId,
            OwnerId = ownerId,
            Name = "Loan APR contact",
        };

        var createOption = await manager.Create(createRequest, _context);
        createOption.IsOk().Should().BeTrue();

        var terms = new LoanTerms
        {
            PrincipalAmount = 10_000.00m,
            APR = 0.05,
            NumberPayments = 12,
            PaymentsPerYear = 12,
        };

        decimal payment = AmortizedLoanTool.CalculatePayment(terms);

        var loanDetail = new LoanDetail
        {
            ContractId = contractId,
            OwnerId = "user1@rental.com",
            FirstPaymentDate = DateTime.UtcNow,
            PrincipalAmount = 10_000.00m,
            Payment = payment,
            APR = terms.APR,
            NumberPayments = terms.NumberPayments,
            PaymentsPerYear = terms.PaymentsPerYear,
        };

        var loanDetailOption = await manager.SetLoanDetail(loanDetail, _context);
        loanDetailOption.IsOk().Should().BeTrue();


    }
}
