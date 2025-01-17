using LoanContract.sdk.Contract;
using LoanContract.sdk.Models;
using LoanContract.sdk.test.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SoftBank.sdk.Application;
using SoftBank.sdk.Models;
using SoftBank.sdk.SoftBank;
using SpinClient.sdk;
using SpinCluster.abstraction;
using SpinTestTools.sdk.ObjectBuilder;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Finance.Finance;
using Toolbox.Tools;
using Toolbox.Tools.Should;
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

    private const string _setup =
        """
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
              "Amount": 100000.00
            },
            {
              "AccountId": "softbank:outlook.com/user2-account/primary",
              "OwnerId": "user2@outlook.com",
              "Description": "Initial deposit",
              "Type": "Credit",
              "Amount": 200000.00
            }
          ],
          "Agents": [
            {
                "AgentId": "agent:test1",
                "Enabled": true
            }
          ]
        }
        """;

    public LoanContractActivityTests(ClusterApiFixture fixture) => _cluster = fixture;

    [Fact]
    public async Task LifecycleTest()
    {
        (ICommandRouterHost commandHost, ScheduleOption scheduleOption) = await Setup();

        var startDate = new DateTime(2023, 1, 1);
        var scheduleClient = _cluster.ServiceProvider.GetRequiredService<SchedulerClient>();

        await PostCreateAccountCommand(startDate, scheduleOption, _smartcId, _contractId, commandHost);
        await CheckSoftBank("softbank:rental.com/user1-account/primary", "user1@rental.com", 100000.00m);
        await CheckSoftBank("softbank:outlook.com/user2-account/primary", "user2@outlook.com", 200000.00m);

        var manager = _cluster.ServiceProvider.GetRequiredService<LoanContractManager>();

        for (int i = 0; i < 12; i++)
        {
            DateTime postedDate = startDate.AddMonths(i + 1);
            await PostInterestCharges(commandHost, postedDate, scheduleOption, scheduleClient);
            await PostPayment(commandHost, postedDate, scheduleOption, scheduleClient);

            var reportOption = await manager.GetReport(_contractId, _ownerId, _context);
            reportOption.IsOk().Should().BeTrue();

            LoanReportModel loanReportModel = reportOption.Return();
            loanReportModel.LedgerItems.Should().NotBeNull();
            loanReportModel.LedgerItems.Count.Should().Be((i + 1) * 2);
        }

        LoanReportModel finalReport = await manager.GetReport(_contractId, _ownerId, _context).Return();
        finalReport.LedgerItems.Count.Should().Be(24);
        finalReport.BalanceItems.Count.Should().Be(12);

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

        Enumerable.SequenceEqual(finalReport.BalanceItems, matchTo).Should().BeTrue();

        await CheckSoftBank("softbank:rental.com/user1-account/primary", "user1@rental.com", 110272.84m);
        await CheckSoftBank("softbank:outlook.com/user2-account/primary", "user2@outlook.com", 200_000.00m - 10272.84m);
    }

    private async Task<(ICommandRouterHost commandHost, ScheduleOption scheduleOption)> Setup()
    {
        var result = await new TestObjectBuilder()
            .SetJson(_setup)
            .SetService(_cluster.ServiceProvider)
            .AddStandard()
            .Build(_context);

        result.IsOk().Should().BeTrue();

        SchedulerClient schedulerClient = _cluster.ServiceProvider.GetRequiredService<SchedulerClient>();
        ScheduleWorkClient scheduleWorkClient = _cluster.ServiceProvider.GetRequiredService<ScheduleWorkClient>();
        var r1 = await schedulerClient.ClearAllWorkSchedules(_schedulerId, scheduleWorkClient, _context);
        r1.IsOk().Should().BeTrue();

        var r2 = await schedulerClient.Delete(_schedulerId, "name@domain.com", _context);
        r2.IsOk().Should().BeTrue();

        ScheduleOption scheduleOption = new ScheduleOption
        {
            AgentId = _agentId,
            SchedulerId = _schedulerId,
            PrincipalId = _ownerId,
            SourceId = "source",
        };

        var contractClient = _cluster.ServiceProvider.GetRequiredService<ContractClient>();
        var contractDelete = await contractClient.Delete(_contractId, _context);
        contractDelete.Assert(x => x.IsOk() || x.IsNotFound(), x => $"delete failed: option={x}");

        ClientOption clientOption = _cluster.ServiceProvider.GetRequiredService<ClientOption>();
        ICommandRouterHost commandHost = LoanContractStartup.CreateSmartcWorkflow(scheduleOption, clientOption).Build();
        return (commandHost, scheduleOption);
    }

    private async Task PostCreateAccountCommand(DateTime startDate, ScheduleOption scheduleOption, string smartcId, string contractId, ICommandRouterHost commandHost)
    {
        await _cluster.ServiceProvider.GetRequiredService<ContractClient>().Delete(contractId, _context);

        var loanAccountDetail = new LoanAccountDetail
        {
            ContractId = contractId,
            OwnerId = _ownerId,
            Name = "Loan APR contact",
            Access = [
                .. LoanAccountDetail.CreatePaymentAccess("user2@outlook.com"),
                .. LoanAccountDetail.CreatePaymentAccess(SoftBankConstants.SoftBankPrincipalId)
                ]
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
            OwnerId = _ownerId,
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


        var scheduleClient = _cluster.ServiceProvider.GetRequiredService<SchedulerClient>();

        ScheduleCreateModel createModel = scheduleOption.CreateSchedule("create", smartcId, loanAccountDetail, loanDetail);
        var result = await scheduleClient.CreateSchedule(createModel, _context);
        result.IsOk().Should().BeTrue();

        int response = await commandHost.Run("run", "--waitFor", "10");
        response.Should().Be(0);

        await CheckWorkScheduleForStatus(scheduleOption, createModel.WorkId, StatusCode.OK);
    }

    private async Task PostInterestCharges(ICommandRouterHost commandHost, DateTime postedDate, ScheduleOption scheduleOption, SchedulerClient scheduleClient)
    {
        var loanInterestRequest = new LoanInterestRequest
        {
            ContractId = _contractId,
            PrincipalId = _ownerId,
            PostedDate = postedDate,
        };

        ScheduleCreateModel interestCharge = scheduleOption.CreateSchedule("interestCharge", _smartcId, loanInterestRequest);
        var postOption = await scheduleClient.CreateSchedule(interestCharge, _context);
        postOption.IsOk().Should().BeTrue(postOption.ToString());

        int response = await commandHost.Run("run", "--waitFor", "10");
        response.Should().Be(0);

        await CheckWorkScheduleForStatus(scheduleOption, interestCharge.WorkId, StatusCode.OK);
    }

    private async Task PostPayment(ICommandRouterHost commandHost, DateTime postedDate, ScheduleOption scheduleOption, SchedulerClient scheduleClient)
    {
        var contractClient = _cluster.ServiceProvider.GetRequiredService<ContractClient>();

        var query = new ContractQueryBuilder().SetPrincipalId(_ownerId).Add<LoanDetail>(true).Build();
        Option<ContractQueryResponse> data = await contractClient.Query(_contractId, query, _context);
        data.IsOk().Should().BeTrue();

        var loanDetailOption = data.Return().GetSingle<LoanDetail>();
        loanDetailOption.IsOk().Should().BeTrue();

        var paymentRequest = new LoanPaymentRequest
        {
            ContractId = _contractId,
            PrincipalId = "user2@outlook.com",
            PostedDate = postedDate,
            PaymentAmount = loanDetailOption.Return().Payment,
        };

        ScheduleCreateModel payment = scheduleOption.CreateSchedule("payment", _smartcId, paymentRequest);
        var paymentOption = await scheduleClient.CreateSchedule(payment, _context);
        paymentOption.IsOk().Should().BeTrue(paymentOption.ToString());

        int response = await commandHost.Run("run", "--waitFor", "10");
        response.Should().Be(0);

        await CheckWorkScheduleForStatus(scheduleOption, payment.WorkId, StatusCode.OK);
    }

    private async Task CheckSoftBank(string accountId, string ownerId, decimal balance)
    {
        SoftBankClient client = _cluster.ServiceProvider.GetRequiredService<SoftBankClient>();

        Option<SbAccountBalance> balanceOption = await client.GetBalance(accountId, ownerId, _context);
        balanceOption.IsOk().Should().BeTrue(balanceOption.ToString());

        balanceOption.Return().PrincipalBalance.Should().Be(balance);
    }

    private async Task CheckWorkScheduleForStatus(ScheduleOption scheduleOption, string workId, StatusCode statusCode)
    {
        var isThereWork = await _cluster.ServiceProvider.GetRequiredService<SchedulerClient>().IsWorkAvailable(scheduleOption.SchedulerId, _context);
        isThereWork.IsError().Should().BeTrue();

        var workScheduleOption = await _cluster.ServiceProvider.GetRequiredService<ScheduleWorkClient>().Get(workId, _context);
        workScheduleOption.IsOk().Should().BeTrue();

        ScheduleWorkModel workSchedule = workScheduleOption.Return();
        workSchedule.Assigned.NotNull().AssignedCompleted.NotNull().StatusCode.Should().Be(StatusCode.OK);

        workSchedule.RunResults.Count.Assert(x => x > 0, x => $"{x} > 0");
        workSchedule.RunResults.All(x => x.StatusCode.IsOk()).Should().BeTrue();
    }
}
