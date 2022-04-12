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

public class ClearingControllerTests
{
    [Fact]
    public async Task SimpleOneToOne_TransferTest_ShouldPass()
    {
        BankAccountClient firstBankAccount = TestApplication.GetHost(BankName.First).GetBankAccountClient();
        BankAccountClient secondBankAccount = TestApplication.GetHost(BankName.Second).GetBankAccountClient();

        BankTransactionClient firstTransactionClient = TestApplication.GetHost(BankName.First).GetBankTransactionClient();
        BankTransactionClient secondTransactionClient = TestApplication.GetHost(BankName.Second).GetBankTransactionClient();

        BankClearingClient clearingClient = TestApplication.GetHost(BankName.First).GetBankClearingClient();

        DocumentId firstAccountId = (DocumentId)"test/bank-first/bankAccount1";
        DocumentId secondAccountId = (DocumentId)"test/bank-second/bankAccount2";

        await CreateBankAccount(firstBankAccount, firstAccountId);
        await CreateBankAccount(secondBankAccount, secondAccountId);
        await TestBankBalance(firstTransactionClient, firstAccountId, 0.0m);
        await TestBankBalance(secondTransactionClient, secondAccountId, 0.0m);

        TrxBatch<TrxRequest> requestBatch = new TrxBatch<TrxRequest>
        {
            Items = new[] {
                new TrxRequest
                {
                    ToId = (string)firstAccountId,
                    FromId = (string)secondAccountId,
                    Amount = 150.00m,
                }
            }
        };

        await clearingClient.Send(requestBatch);
    }

    private async Task CreateBankAccount(BankAccountClient client, DocumentId bankId)
    {
        await client.Delete(bankId);

        BankAccount entry = new BankAccount
        {
            AccountId = bankId.Path,
            AccountName = bankId.Path.Split('/').Last(),
            AccountNumber = Guid.NewGuid().ToString(),
        };

        await client.Set(entry);
    }

    private async Task TestBankBalance(BankTransactionClient client, DocumentId bankId, decimal shouldBeBalance)
    {
        TrxBalance? balanceTrx = await client.GetBalance(bankId);
        balanceTrx.Should().NotBeNull();
        balanceTrx!.Balance.Should().Be(shouldBeBalance);
    }
}
