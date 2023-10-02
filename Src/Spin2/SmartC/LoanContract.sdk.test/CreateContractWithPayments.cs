using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using LoanContract.sdk.Contract;
using LoanContract.sdk.Models;
using LoanContract.sdk.test.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SoftBank.sdk.Models;
using SoftBank.sdk.SoftBank;
using SpinTestTools.sdk.ObjectBuilder;
using Toolbox.Extensions;
using Toolbox.Finance.Finance;
using Toolbox.Types;

namespace LoanContract.sdk.test;

[Collection("config-Test")]
public class CreateContractWithPayments : IClassFixture<ClusterApiFixture>
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
                "SubscriptionId": "subscription:rentalManagment2",
                "Name": "Rental Management",
                "ContactName": "user1@rental2.com",
                "Email": "admin@rental2.com"
              }
            ],
           "Tenants": [
              {
                "TenantId": "tenant:rental2.com",
                "Subscription": "Rental tenant",
                "Domain": "rental2.com",
                "SubscriptionId": "subscription:rentalManagment2",
                "ContactName": "Admin",
                "Email": "admin@rental2.com",
                "Enabled": true
              }        
            ],
           "Users": [
              {
                "UserId": "user:user1@rental2.com",
                "PrincipalId": "user1@rental2.com",
                "DisplayName": "User 2",
                "FirstName": "user1first",
                "LastName": "user1last"
              },
              {
                "UserId": "user:user4@outlook.com",
                "PrincipalId": "user4@outlook.com",
                "DisplayName": "External user 2",
                "FirstName": "user1first",
                "LastName": "user1last"
              }
            ],
           "SbAccounts": [
              {
                "AccountId": "softbank:rental2.com/user1-account/primary",
                "OwnerId": "user1@rental2.com",
                "Name": "User1"
              },
              {
                "AccountId": "softbank:outlook.com/user4-account/primary",
                "OwnerId": "user4@outlook.com",
                "Name": "User4 outlook.com",
                "AccessRights": [
                    {
                        "BlockType": "SbLedgerItem",
                        "Grant": "ReadWrite",
                        "PrincipalId": "user1@rental2.com"
                    }
                ]
              }
            ],
          "LedgerItems": [
            {
              "AccountId": "softbank:rental2.com/user1-account/primary",
              "OwnerId": "user1@rental2.com",
              "Description": "Initial deposit",
              "Type": "Credit",
              "Amount": 100000.00
            },
            {
              "AccountId": "softbank:outlook.com/user4-account/primary",
              "OwnerId": "user4@outlook.com",
              "Description": "Initial deposit",
              "Type": "Credit",
              "Amount": 200000.00
            }
          ]
        }
        """;

    public CreateContractWithPayments(ClusterApiFixture fixture)
    {
        _cluster = fixture;
    }

    [Fact]
    public async Task LifecycleTest()
    {
        var manager = _cluster.ServiceProvider.GetRequiredService<LoanContractManager>();

        var builder = new TestObjectBuilder()
            .SetJson(_setup)
            .SetService(_cluster.ServiceProvider)
            .AddStandard();

        var result = await builder.Build(_context);
        result.IsOk().Should().BeTrue();

        const string contractId = "contract:rental2.com/loan/loanToUser1";
        const string ownerId = "user1@rental2.com";

        var startDate = new DateTime(2023, 1, 1);

        await CreateAccount(startDate, contractId, ownerId);
        await CheckSoftBank("softbank:rental2.com/user1-account/primary", "user1@rental2.com", 100_000.00m);
        await CheckSoftBank("softbank:outlook.com/user4-account/primary", "user4@outlook.com", 200_000.00m);

        var r1 = await manager.GetReport(contractId, ownerId, _context);
        r1.IsOk().Should().BeTrue();

        decimal payment = r1.Return().LoanDetail.Payment;

        for (int i = 0; i < 12; i++)
        {
            DateTime postedDate = startDate.AddMonths(i + 1);
            var postOption = await manager.MakePayment(contractId, ownerId, postedDate, payment, $"r{i}", _context);
            postOption.IsOk().Should().BeTrue();

            var reportOption = await manager.GetReport(contractId, ownerId, _context);
            reportOption.IsOk().Should().BeTrue();

            LoanReportModel loanReportModel = reportOption.Return();
            loanReportModel.LedgerItems.Should().NotBeNull();
            loanReportModel.LedgerItems.Count.Should().Be((i + 1) * 2);
            loanReportModel.BalanceItems.Count.Should().Be(i + 1);
        }

        var finalReportOption = await manager.GetReport(contractId, ownerId, _context);
        finalReportOption.IsOk().Should().BeTrue();

        var matchTo = new[]
        {
            new LedgerBalanceItem { PostedDate = new DateTime(2023, 2, 1), CreditCharge = 42.47m, Payment = 856.07m, ToPrincipal = 813.60m, PrincipalBalance = 9186.40m },
            new LedgerBalanceItem { PostedDate = new DateTime(2023, 3, 1), CreditCharge = 35.24m, Payment = 856.07m, ToPrincipal = 820.83m, PrincipalBalance = 8365.57m },
            new LedgerBalanceItem { PostedDate = new DateTime(2023, 4, 1), CreditCharge = 35.53m, Payment = 856.07m, ToPrincipal = 820.54m, PrincipalBalance = 7545.03m },
            new LedgerBalanceItem { PostedDate = new DateTime(2023, 5, 1), CreditCharge = 31.01m, Payment = 856.07m, ToPrincipal = 825.06m, PrincipalBalance = 6719.97m },
            new LedgerBalanceItem { PostedDate = new DateTime(2023, 6, 1), CreditCharge = 28.54m, Payment = 856.07m, ToPrincipal = 827.53m, PrincipalBalance = 5892.44m },
            new LedgerBalanceItem { PostedDate = new DateTime(2023, 7, 1), CreditCharge = 24.22m, Payment = 856.07m, ToPrincipal = 831.85m, PrincipalBalance = 5060.59m },
            new LedgerBalanceItem { PostedDate = new DateTime(2023, 8, 1), CreditCharge = 21.49m, Payment = 856.07m, ToPrincipal = 834.58m, PrincipalBalance = 4226.01m },
            new LedgerBalanceItem { PostedDate = new DateTime(2023, 9, 1), CreditCharge = 17.95m, Payment = 856.07m, ToPrincipal = 838.12m, PrincipalBalance = 3387.89m },
            new LedgerBalanceItem { PostedDate = new DateTime(2023, 10, 1), CreditCharge = 13.92m, Payment = 856.07m, ToPrincipal = 842.15m, PrincipalBalance = 2545.74m },
            new LedgerBalanceItem { PostedDate = new DateTime(2023, 11, 1), CreditCharge = 10.81m, Payment = 856.07m, ToPrincipal = 845.26m, PrincipalBalance = 1700.48m },
            new LedgerBalanceItem { PostedDate = new DateTime(2023, 12, 1), CreditCharge = 6.99m, Payment = 856.07m, ToPrincipal = 849.08m, PrincipalBalance = 851.40m },
            new LedgerBalanceItem { PostedDate = new DateTime(2024, 1, 1), CreditCharge = 3.62m, Payment = 856.07m, ToPrincipal = 852.45m, PrincipalBalance = -1.05m },
        };

        Enumerable.SequenceEqual(finalReportOption.Return().BalanceItems, matchTo).Should().BeTrue();

        await CheckSoftBank("softbank:rental2.com/user1-account/primary", "user1@rental2.com", 110272.84m);
        await CheckSoftBank("softbank:outlook.com/user4-account/primary", "user4@outlook.com", 200_000.00m - 10272.84m);

        var finalizeResult = await builder.DeleteAll(_context);
        finalizeResult.IsOk().Should().BeTrue();
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
            OwnerId = "user1@rental2.com",
            OwnerSoftBankId = "softbank:rental2.com/user1-account/primary",
            FirstPaymentDate = startDate,
            PrincipalAmount = 10_000.00m,
            Payment = payment,
            APR = terms.APR,
            NumberPayments = terms.NumberPayments,
            PaymentsPerYear = terms.PaymentsPerYear,
            PartyPrincipalId = "user4@outlook.com",
            PartySoftBankId = "softbank:outlook.com/user4-account/primary",
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

        balance.Should().Be(balanceOption.Return().PrincipalBalance);
    }
}
