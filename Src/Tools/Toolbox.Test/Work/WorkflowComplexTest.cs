using FluentAssertions;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Work;
using Xunit;

namespace Toolbox.Test.Work;

public class WorkflowComplexTest
{
    [Fact]
    public async Task GivenBankScenario_WhenProcessTransaction_ShouldPass()
    {
        IServiceProvider serviceProvider = new ServiceCollection()
            .AddSingleton<Bank>()
            .AddSingleton<FundTransfer>()
            .AddSingleton<Balance>()
            .AddSingleton<AddTransaction>()
            .AddSingleton<WorkflowBuilder>()
            .AddLogging(config =>
            {
                config.AddDebug();
                config.AddFilter(x => true);
            })
            .BuildServiceProvider();

        Workflow workflow = serviceProvider.GetRequiredService<WorkflowBuilder>()
            .Add<FundTransfer>()
            .Add<Balance>()
            .Add<AddTransaction>()
            .Build();

        Bank bank = serviceProvider.GetRequiredService<Bank>();
        bank.Add(new AccountDetail(AccountId_001, "fed", true, 100.00m));

        var balance_001 = await workflow.Send<Balance, BalanceRequest, BalanceResponse>(new BalanceRequest(AccountId_001));
        balance_001.Should().NotBeNull();
        balance_001.AccountId.Should().Be(AccountId_001);
        balance_001.Balance.Should().Be(100.00m);

        var balance_002 = await workflow.Send<Balance, BalanceRequest, BalanceResponse>(new BalanceRequest(AccountId_002));
        balance_002.Should().NotBeNull();
        balance_002.AccountId.Should().Be(AccountId_002);
        balance_002.Balance.Should().Be(0.00m);

        var moveMoneyRequest = new MoveMoneyRequest(AccountId_001, AccountId_002, 35.00m);
        var moveMoneyResponse = await workflow.Send<FundTransfer, MoveMoneyRequest, MoveMoneyResponse>(moveMoneyRequest);
        moveMoneyResponse.Should().NotBeNull();

        var balance_001_A = await workflow.Send<Balance, BalanceRequest, BalanceResponse>(new BalanceRequest(AccountId_001));
        balance_001_A.Should().NotBeNull();
        balance_001_A.AccountId.Should().Be(AccountId_001);
        balance_001_A.Balance.Should().Be(65.00m);

        var balance_002_A = await workflow.Send<Balance, BalanceRequest, BalanceResponse>(new BalanceRequest(AccountId_002));
        balance_002_A.Should().NotBeNull();
        balance_002_A.AccountId.Should().Be(AccountId_002);
        balance_002_A.Balance.Should().Be(35.00m);
    }


    private const string AccountId_001 = "Account_001";
    private const string AccountId_002 = "Account_002";

    private record BalanceRequest(string AccountId);
    private record BalanceResponse(string AccountId, decimal Balance);
    private record MoveMoneyRequest(string FromAccountId, string ToAccountId, decimal Amount);
    private record MoveMoneyResponse(MoveMoneyRequest Request, bool Success);
    private record AccountDetail(string AccountId, string? FromAccountId, bool Credit, decimal Amount);
    private record AccountDetailResponse(string AccountId, bool Success);

    private record Account()
    {
        public string AccountId { get; init; } = null!;
        public List<AccountDetail> Details { get; init; } = new List<AccountDetail>();
    }

    private class Bank
    {
        public List<Account> Accounts { get; init; } = new List<Account>();
        public void Add(AccountDetail detail) => GetAccount(detail.AccountId).Details.Add(detail);
        public decimal GetBalance(string accountId) => Accounts
            .Where(x => x.AccountId == accountId)
            .SelectMany(x => x.Details)
            .Sum(x => x.Credit switch
            {
                false => 0 - x.Amount,
                true => x.Amount,
            });

        private Account GetAccount(string accountId) => Accounts.FirstOrDefault(x => x.AccountId == accountId) ?? AddAccount(accountId);
        private Account AddAccount(string accountId) => new Account { AccountId = accountId }.Action(x => Accounts.Add(x));
    }


    private class FundTransfer : WorkflowActivity<MoveMoneyRequest, MoveMoneyResponse>
    {
        private readonly Bank _bank;
        public FundTransfer(Bank bank) => _bank = bank;

        protected override async Task<MoveMoneyResponse> Send(MoveMoneyRequest request, Workflow workflow)
        {
            var balanceRequest = new BalanceRequest(AccountId_001);
            var balanceResponse = await workflow.Send<Balance, BalanceRequest, BalanceResponse>(balanceRequest);

            if (balanceResponse.Balance - request.Amount <= 0) return new MoveMoneyResponse(request, false);

            var request1 = new AccountDetail(request.FromAccountId, request.ToAccountId, false, request.Amount);
            var request1Response = await workflow.Send<AddTransaction, AccountDetail, AccountDetailResponse>(request1);
            request1Response.Success.Should().BeTrue();

            var request2 = new AccountDetail(request.ToAccountId, request.FromAccountId, true, request.Amount);
            var request2Response = await workflow.Send<AddTransaction, AccountDetail, AccountDetailResponse>(request2);
            request2Response.Success.Should().BeTrue();

            return new MoveMoneyResponse(request, true);
        }
    }

    private class Balance : WorkflowActivity<BalanceRequest, BalanceResponse>
    {
        private readonly Bank _bank;
        public Balance(Bank bank) => _bank = bank;

        protected override Task<BalanceResponse> Send(BalanceRequest request, Workflow workflow)
        {
            var response = new BalanceResponse(request.AccountId, _bank.GetBalance(request.AccountId));
            return Task.FromResult(response);
        }
    }

    private class AddTransaction : WorkflowActivity<AccountDetail, AccountDetailResponse>
    {
        private readonly Bank _bank;
        public AddTransaction(Bank bank) => _bank = bank;

        protected override Task<AccountDetailResponse> Send(AccountDetail request, Workflow workflow)
        {
            _bank.Add(request);
            var response = new AccountDetailResponse(request.AccountId, true);
            return Task.FromResult(response);
        }
    }
}
