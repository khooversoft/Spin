using FluentAssertions;
using LoanContract.sdk.Contract;
using LoanContract.sdk.Models;
using LoanContract.sdk.test.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SoftBank.sdk.Models;
using SoftBank.sdk.SoftBank;
using SpinAgent.sdk;
using SpinClient.sdk;
using SpinTestTools.sdk.ObjectBuilder;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Finance.Finance;
using Toolbox.Types;

namespace LoanContract.sdk.test;

[Collection("config-Test")]
public class LoanContractActivityTests : IClassFixture<ClusterApiFixture>
{
    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    const string _contractId = "contract:rental.com/loan/loanToUser1";
    const string _smartcId = "smartc:rental.com/loanToUser1-contract";
    const string _ownerId = "user1@rental.com";
    const string _schedulerId = "scheduler:test";
    const string _agentId = "agent:test1";

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

    public LoanContractActivityTests(ClusterApiFixture fixture) => _cluster = fixture;

    [Fact]
    public async Task LifecycleTest()
    {
        await Setup();

        AgentOption agentOption = new AgentOption
        {
            ClusterApiUri = _cluster.ServiceProvider.GetRequiredService<TestOption>().ClusterApiUri,
            AgentId = _agentId,
            SchedulerId = _schedulerId,
            PrincipalId = _ownerId,
            SourceId = "source",
        };

        AgentSession agentSession = ActivatorUtilities.CreateInstance<AgentSession>(_cluster.ServiceProvider, agentOption);

        var startDate = new DateTime(2023, 1, 1);

        await PostCreateAccountCommand(agentSession, startDate, _ownerId, _smartcId, _contractId);
        await CheckSoftBank("softbank:rental.com/user1-account/primary", "user1@rental.com", 100.00m);
        await CheckSoftBank("softbank:outlook.com/user2-account/primary", "user2@outlook.com", 2000.00m);

        var manager = _cluster.ServiceProvider.GetRequiredService<LoanContractManager>();

        for (int i = 0; i < 12; i++)
        {
            DateTime postedDate = startDate.AddMonths(i + 1);

            var paymentRequest = new LoanPaymentRequest
            {
                ContractId = _contractId,
                PrincipalId = _ownerId,
                PostedDate = postedDate,
            };

            var postOption = await agentSession.CreateSchedule("interestCharge", _smartcId, paymentRequest, _context);
            postOption.IsOk().Should().BeTrue(postOption.ToString());

            var reportOption = await manager.GetReport(_contractId, _ownerId, _context);
            reportOption.IsOk().Should().BeTrue();

            LoanReportModel loanReportModel = reportOption.Return();
            loanReportModel.LedgerItems.Should().NotBeNull();
            loanReportModel.LedgerItems.Count.Should().Be(i + 1);
        }

        //LoanReportModel finalReport = await manager.GetReport(contractId, ownerId, _context).Return();
        //finalReport.LedgerItems.Count.Should().Be(12);

        //var testSet = new decimal[]
        //{
        //    -42.47m, -38.52m, -42.81m, -41.60m, -43.17m, -41.95m,
        //    -43.53m, -43.71m, -42.48m, -44.08m, -42.84m, -44.45m,
        //};

        //finalReport.LedgerItems.WithIndex().ForEach(x =>
        //{
        //    x.Item.ContractId.Should().Be(contractId);
        //    x.Item.OwnerId.Should().Be(ownerId);
        //    x.Item.Description.Should().Be("Interest charge");
        //    x.Item.Type.Should().Be(LoanLedgerType.Debit);
        //    x.Item.TrxType.Should().Be(LoanTrxType.InterestCharge);
        //    x.Item.NaturalAmount.Should().Be(testSet[x.Index]);
        //});
    }

    private async Task Setup()
    {
        var result = await new TestObjectBuilder()
            .SetJson(_setup)
            .SetService(_cluster.ServiceProvider)
            .AddStandard()
            .Build(_context);

    }

    private async Task PostCreateAccountCommand(AgentSession agentSession, DateTime startDate, string ownerId, string smartcId, string contractId)
    {
        await _cluster.ServiceProvider.GetRequiredService<ContractClient>().Delete(contractId, _context);

        var loanAccountDetail = new LoanAccountDetail
        {
            ContractId = contractId,
            OwnerId = ownerId,
            Name = "Loan APR contact",
        };

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

        var result = await agentSession.CreateSchedule("create", smartcId, loanAccountDetail, loanDetail, _context);
        result.IsOk().Should().BeTrue();

        //var builder = LoanContractStartup.CreateInMemory
    }

    //private async Task GetAssignment

    private async Task CheckSoftBank(string accountId, string ownerId, decimal balance)
    {
        SoftBankClient client = _cluster.ServiceProvider.GetRequiredService<SoftBankClient>();

        Option<SbAccountBalance> balanceOption = await client.GetBalance(accountId, ownerId, _context);
        balanceOption.IsOk().Should().BeTrue();

        (balance == balanceOption.Return().PrincipalBalance).Should().BeTrue();
    }
}
