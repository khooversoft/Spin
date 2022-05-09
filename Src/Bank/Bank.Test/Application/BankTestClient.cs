using Bank.Abstractions.Model;
using Bank.sdk.Client;
using FluentAssertions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Abstractions;
using Toolbox.Tools;

namespace Bank.Test.Application;

internal class BankTestClient
{
    private readonly BankTestTool _bankTestTool;
    private BankAccountClient _bankAccountClient = null!;
    private BankTransactionClient _bankTransactionClient = null!;
    private BankClearingClient _clearingClient = null!;

    public BankTestClient(BankTestTool bankTestTool, BankName bankName)
    {
        _bankTestTool = bankTestTool;
        BankName = bankName;

        BankAccountId = _bankTestTool.GetBankAcountId(bankName);
    }

    public BankName BankName { get; }

    public DocumentId BankAccountId { get; }

    public async Task Start()
    {
        _bankAccountClient = TestApplication.GetHost(BankName).GetBankAccountClient();
        _bankTransactionClient = TestApplication.GetHost(BankName).GetBankTransactionClient();
        _clearingClient = TestApplication.GetHost(BankName).GetBankClearingClient();

        await CreateBankAccount();
        await TestBankBalance(0.0m);
    }

    public async Task Send(TrxBatch<TrxRequest> batch) => await _clearingClient.Send(batch);

    public Task PullMoney(decimal amount, decimal newLocalBalance, decimal newPartnerBalance) => MoveMoney(new[] { Math.Abs(amount) }, newLocalBalance, newPartnerBalance);

    public Task PushMoney(decimal amount, decimal newLocalBalance, decimal newPartnerBalance) => MoveMoney(new[] { -Math.Abs(amount) }, newLocalBalance, newPartnerBalance);

    public async Task MoveMoney(decimal[] amounts, decimal newLocalBalance, decimal newPartnerBalance)
    {
        BankTestClient partnerClient = _bankTestTool.GetPartnerBankClient(BankName);

        TrxBatch<TrxRequest> requestBatch = new TrxBatch<TrxRequest>
        {
            Items = amounts.Select(x => new TrxRequest
            {
                ToId = x >= 0 ? (string)BankAccountId : (string)partnerClient.BankAccountId,
                FromId = x >= 0 ? (string)partnerClient.BankAccountId : (string)BankAccountId,
                Amount = Math.Abs(x),
            }).ToList(),
        };

        await Send(requestBatch);

        foreach (var item in requestBatch.Items)
        {
            await partnerClient.WatchForRequestChange(item.Id);
            await WatchForResponseChange(item.Id);
        }

        await TestBankBalance(newLocalBalance);
        await partnerClient.TestBankBalance(newPartnerBalance);
    }

    public async Task CreateBankAccount()
    {
        await _bankAccountClient.Delete(BankAccountId);

        BankAccount entry = new BankAccount
        {
            AccountId = BankAccountId.Path,
            AccountName = BankAccountId.Path.Split('/').Last(),
            AccountNumber = Guid.NewGuid().ToString(),
        };

        await _bankAccountClient.Set(entry);
    }

    public async Task TestBankBalance(decimal shouldBeBalance)
    {
        TrxBalance? balanceTrx = await _bankTransactionClient.GetBalance(BankAccountId);
        balanceTrx.Should().NotBeNull();
        balanceTrx!.Balance.Should().Be(shouldBeBalance);
    }

    public async Task AddToBalance(decimal amount)
    {
        BankTestClient partnerClient = _bankTestTool.GetPartnerBankClient(BankName);

        TrxBatch<TrxRequest> requestBatch = new TrxBatch<TrxRequest>
        {
            Items = Enumerable.Empty<TrxRequest>().Append(new TrxRequest
            {
                ToId = amount >= 0 ? BankAccountId.Path : partnerClient.BankAccountId.Path,
                FromId = amount >= 0 ? partnerClient.BankAccountId.Path : BankAccountId.Path,
                Amount = Math.Abs(amount),
            }).ToList(),
        };

        TrxBatch<TrxRequestResponse> response = await _bankTransactionClient.Set(requestBatch);
        response.Should().NotBeNull();
        response.Items.Count.Should().Be(1);
        response.Items.All(x => x.Status == TrxStatus.Success).Should().BeTrue();
    }

    public Task WatchForRequestChange(string id) =>
        WatchForChange(x => x.Requests.Any(y => y.Id == id));

    public Task WatchForResponseChange(string id) =>
        WatchForChange(x => x.Responses.Any(y => y.Reference.Id == id));

    private Task WatchForChange(Func<BankAccount, bool> test)
    {
        var tcs = new TaskCompletionSource<bool>();
        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(100));
        tokenSource.Token.Register(() => tcs.SetResult(false));

        _ = Task.Run(() =>
        {
            while (!tokenSource.IsCancellationRequested)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));

                BankAccount entry = _bankAccountClient.Get(BankAccountId, tokenSource.Token).Result
                    .NotNull($"Bank not found: {BankAccountId}");

                if (test(entry))
                {
                    tcs.SetResult(true);
                    return;
                }
            }
        });

        return tcs.Task;
    }
}

