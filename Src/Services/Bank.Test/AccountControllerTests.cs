using Bank.sdk.Client;
using Bank.sdk.Model;
using Bank.Test.Application;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Document;
using Toolbox.Extensions;
using Toolbox.Model;
using Toolbox.Tools;
using Xunit;

namespace Bank.Test;

public class AccountControllerTests
{
    [Fact]
    public async Task GivenAccount_WhenRoundTrip_ShouldSucceed()
    {
        BankAccountClient client = TestApplication.GetHost(BankName.First).GetBankAccountClient();

        DocumentId documentId = (DocumentId)"test/bank/bankAccount1";

        await client.Delete(documentId);

        BankAccount entry = new BankAccount
        {
            AccountId = documentId.Path,
            AccountName = "testBankAccount",
            AccountNumber = Guid.NewGuid().ToString(),
        };

        await client.Set(entry);

        BankAccount? account = await client.Get(documentId);
        account.Should().NotBeNull();
        account!.AccountId.Should().Be(entry.AccountId);
        account.AccountName.Should().Be(entry.AccountName);
        account.AccountNumber.Should().Be(entry.AccountNumber);
        account.Transactions.Count.Should().Be(0);

        account = account with
        {
            AccountName = "newBankAccount",
            Transactions = new[]
            {
                new TrxRecord { Type = TrxType.Credit, Amount = 100.0m},
                new TrxRecord { Type = TrxType.Debit, Amount = 41.0m},
            }.ToList()            
        };

        await client.Set(account);

        BankAccount? account1 = await client.Get(documentId);
        account1.Should().NotBeNull();
        account1!.AccountId.Should().Be(account.AccountId);
        account1.AccountName.Should().Be(account.AccountName);
        account1.AccountNumber.Should().Be(account.AccountNumber);
        account1.Transactions.Count.Should().Be(2);
        (account1.Transactions[0] == account.Transactions[0]).Should().BeTrue();
        (account1.Transactions[1] == account.Transactions[1]).Should().BeTrue();

        var query = new QueryParameter
        {
            Filter = "test/bank"
        };

        BatchSetCursor<DatalakePathItem> cursor = client.Search(query);
        cursor.Should().NotBeNull();

        BatchSet<DatalakePathItem> batchSet = await cursor.ReadNext();
        batchSet.Should().NotBeNull();
        batchSet.Records.Any(x => x.Name.EndsWith(documentId.Path)).Should().BeTrue();

        await client.Delete(documentId);
    }
}
