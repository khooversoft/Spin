using Bank.sdk.Client;
using Bank.sdk.Model;
using Bank.Test.Application;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Document;
using Xunit;

namespace Bank.Test;

public class TransactionControllerTests
{
    private readonly DocumentId _toAccountId = (DocumentId)"Bank-First/bankAccount1";
    private readonly DocumentId _fromAccountId = (DocumentId)"Bank-Second/bankAccount1";

    [Fact]
    public async Task GivenBankAccount_WhenTransactionsSet_ShouldSucceed()
    {
        BankAccountClient accountClient = TestApplication.GetHost(BankName.First).GetBankAccountClient();
        BankTransactionClient transactionClient = TestApplication.GetHost(BankName.First).GetBankTransactionClient();

        await accountClient.Delete(_toAccountId);

        BankAccount entry = new BankAccount
        {
            AccountId = _toAccountId.Path,
            AccountName = _fromAccountId.Path,
            AccountNumber = Guid.NewGuid().ToString(),
        };

        await accountClient.Set(entry);

        TrxBalance? balanceTrx = await transactionClient.GetBalance(_toAccountId);
        balanceTrx.Should().NotBeNull();
        balanceTrx!.Balance.Should().Be(0.0m);

        decimal finalBalance = 270.42m;
        await ApplyTransaction(transactionClient, balance: 155.15m, 100.0m, 75.15m, -20.0m);
        await ApplyTransaction(transactionClient, balance: 240.30m, -10.0m, 75.15m, 20.0m);
        await ApplyTransaction(transactionClient, balance: finalBalance, -20.0m, 45.0m, 5.12m);

        BankAccount? readAccount = await accountClient.Get(_toAccountId);
        readAccount.Should().NotBeNull();

        readAccount!.AccountId.Should().Be(entry.AccountId);
        readAccount.AccountName.Should().Be(entry.AccountName);
        readAccount.AccountNumber.Should().Be(entry.AccountNumber);
        readAccount.Transactions.Count.Should().Be(9);
        readAccount.Balance().Should().Be(finalBalance);

        await accountClient.Delete(_toAccountId);
    }

    private async Task ApplyTransaction(BankTransactionClient transactionClient, decimal balance, params decimal[] amounts)
    {
        TrxBatch<TrxRequest> requestBatch = new TrxBatch<TrxRequest>
        {
            Items = amounts.Select(x => new TrxRequest
            {
                ToId = x >= 0 ? _toAccountId.Path : _fromAccountId.Path,
                FromId = x >= 0 ? _fromAccountId.Path : _toAccountId.Path,
                Amount = Math.Abs(x),
            }).ToList(),
        };

        TrxBatch<TrxRequestResponse> response = await transactionClient.Set(requestBatch);
        response.Should().NotBeNull();
        response.Items.Count.Should().Be(amounts.Length);
        response.Items.All(x => x.Status == TrxStatus.Success).Should().BeTrue();

        response.Items
            .Zip(requestBatch.Items)
            .All(x => x.First.Reference.Id == x.Second.Id)
            .Should().BeTrue();

        TrxBalance? balanceTrx = await transactionClient.GetBalance(_toAccountId);
        balanceTrx.Should().NotBeNull();
        balanceTrx!.Balance.Should().Be(balance);
    }
}
