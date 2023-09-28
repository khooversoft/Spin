using System.Diagnostics;
using FluentAssertions;
using LoanContract.sdk.Contract;
using LoanContract.sdk.Models;
using LoanContract.sdk.test.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SoftBank.sdk.Models;
using SoftBank.sdk.SoftBank;
using SpinTestTools.sdk.ObjectBuilder;
using Toolbox.Finance.Finance;
using Toolbox.Types;

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
                "Properties": {
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
                "UserId": "user:user2@outlook.com",
                "PrincipalId": "user2@outlook.com",
                "DisplayName": "External user 1",
                "FirstName": "user1first",
                "LastName": "user1last"
              }
            ],
           "SbAccounts": [
              {
                "AccountId": "softbank:rental.com/user1-account/primary",
                "OwnerId": "user1@rental.com",
                "Name": "User1"
              },
              {
                "AccountId": "softbank:outlook.com/user2-account/primary",
                "OwnerId": "user2@outlook.com",
                "Name": "User2 outlook.com"
              }
            ],
          "LedgerItems": [
            {
              "AccountId": "softbank:rental.com/user1-account/primary",
              "OwnerId": "user1@rental.com",
              "Description": "Initial deposit",
              "Type": "Credit",
              "Amount": 100.00
            },
            {
              "AccountId": "softbank:outlook.com/user2-account/primary",
              "OwnerId": "user2@outlook.com",
              "Description": "Initial deposit",
              "Type": "Credit",
              "Amount": 2000.00
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

        var startDate = new DateTime(2023, 1, 1);

        await CreateAccount(startDate, contractId, ownerId);
        await CheckSoftBank("softbank:rental.com/user1-account/primary", "user1@rental.com", 100.00m);
        await CheckSoftBank("softbank:outlook.com/user2-account/primary", "user2@outlook.com", 2000.00m);

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 12; i++)
        {
            DateTime postedDate = startDate.AddMonths(i + 1);
            var postOption = await manager.PostInterestCharge(contractId, ownerId, postedDate, _context);
            postOption.IsOk().Should().BeTrue();

            var reportOption = await manager.GetReport(contractId, ownerId, _context);
            reportOption.IsOk().Should().BeTrue();

            LoanReportModel loanReportModel = reportOption.Return();
            loanReportModel.LedgerItems.Should().NotBeNull();
            loanReportModel.LedgerItems.Count.Should().Be(i + 1);
        }

        sw.Stop();
        var duration = sw.Elapsed;
    }

    private async Task CreateAccount(DateTime startDate, string contractId, string ownerId)
    {
        var manager = _cluster.ServiceProvider.GetRequiredService<LoanContractManager>();

        await manager.Delete(contractId, _context);

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
            OwnerSoftBankId = "softbank:rental.com/user1-account/primary",
            FirstPaymentDate = startDate,
            PrincipalAmount = 10_000.00m,
            Payment = payment,
            APR = terms.APR,
            NumberPayments = terms.NumberPayments,
            PaymentsPerYear = terms.PaymentsPerYear,
            PartyPrincipalId = "user2@outlook.com",
            PartySoftBankId = "softbank:outlook.com/user2-account/primary",
        };

        var loanDetailOption = await manager.SetLoanDetail(loanDetail, _context);
        loanDetailOption.IsOk().Should().BeTrue();

        var reportOption = await manager.GetReport(contractId, ownerId, _context);
        reportOption.IsOk().Should().BeTrue();

        LoanReportModel loanReportModel = reportOption.Return();
        loanReportModel.Should().NotBeNull();
        loanReportModel.LoanDetail.Should().NotBeNull();
        (loanDetail == loanReportModel.LoanDetail).Should().BeTrue();
    }

    private async Task CheckSoftBank(string accountId, string ownerId, decimal balance)
    {
        SoftBankClient client = _cluster.ServiceProvider.GetRequiredService<SoftBankClient>();

        Option<SbAccountBalance> balanceOption = await client.GetBalance(accountId, ownerId, _context);
        balanceOption.IsOk().Should().BeTrue();

        (balance == balanceOption.Return().PrincipalBalance).Should().BeTrue();
    }
}
