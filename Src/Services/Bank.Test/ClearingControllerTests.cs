using Bank.sdk.Client;
using Bank.Test.Application;
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

        BankClearingClient clearingClient = TestApplication.GetHost(BankName.First).GetBankClearingClient();

        DocumentId documentId = (DocumentId)"test/bank/bankAccount2";

        await firstBankAccount.Delete(documentId);

        BankAccount entry = new BankAccount
        {
            AccountId = documentId.Path,
            AccountName = "testBankAccount2",
            AccountNumber = Guid.NewGuid().ToString(),
        };

        await accountClient.Set(entry);



    }
}
