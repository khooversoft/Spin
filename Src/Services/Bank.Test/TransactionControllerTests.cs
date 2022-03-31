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
    [Fact]
    public async Task GivenBankAccount_WhenTransactionsSet_ShouldSucceed()
    {
        BankAccountClient accountClient = TestApplication.GetHost(BankName.First).GetBankAccountClient();
        BankTransactionClient transactionClient = TestApplication.GetHost(BankName.First).GetBankTransactionClient();

        DocumentId documentId = (DocumentId)"Bank-First/bankAccount1";

        await accountClient.Delete(documentId);

        BankAccount entry = new BankAccount
        {
            AccountId = documentId.Path,
            AccountName = "Bank-First/bankAccount1",
            AccountNumber = Guid.NewGuid().ToString(),
        };

        await accountClient.Set(entry);

        TrxBalance? balanceTrx = await transactionClient.GetBalance(documentId);
        balanceTrx.Should().NotBeNull();
        balanceTrx!.Balance.Should().Be(0.0m);

        decimal finalBalance = 270.42m;
        await ApplyTransaction(transactionClient, documentId, balance: 155.15m, 100.0m, 75.15m, -20.0m);
        await ApplyTransaction(transactionClient, documentId, balance: 240.30m, -10.0m, 75.15m, 20.0m);
        await ApplyTransaction(transactionClient, documentId, balance: finalBalance, -20.0m, 45.0m, 5.12m);

        BankAccount? readAccount = await accountClient.Get(documentId);
        readAccount.Should().NotBeNull();

        readAccount!.AccountId.Should().Be(entry.AccountId);
        readAccount.AccountName.Should().Be(entry.AccountName);
        readAccount.AccountNumber.Should().Be(entry.AccountNumber);
        readAccount.Transactions.Count.Should().Be(9);
        readAccount.Balance().Should().Be(finalBalance);

        await accountClient.Delete(documentId);
    }

    private async Task ApplyTransaction(BankTransactionClient transactionClient, DocumentId documentId, decimal balance, params decimal[] amounts)
    {
        TrxBatch<TrxRequest> requestBatch = new TrxBatch<TrxRequest>
        {
            Items = amounts.Select(x => new TrxRequest
            {
                ToId = documentId.Path,
                FromId = "Bank-First/fromAccount",
                Type = x >= 0 ? TrxType.Credit : TrxType.Debit,
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

        TrxBalance? balanceTrx = await transactionClient.GetBalance(documentId);
        balanceTrx.Should().NotBeNull();
        balanceTrx!.Balance.Should().Be(balance);
    }
}
