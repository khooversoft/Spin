using Bank.Test.Application;
using System.Threading.Tasks;
using Xunit;

namespace Bank.Test;

public class ClearingControllerTests
{
    [Fact]
    public async Task Push_TransferTest_ShouldPass()
    {
        await TestApplication.ResetQueues();

        var bankTools = new BankTestTool();
        await bankTools.Start();

        await bankTools.BankFirst.AddToBalance(200.00m);
        await bankTools.BankFirst.TestBankBalance(200.00m);

        await bankTools.BankFirst.PushMoney(150.00m, 50.00m, 150.00m);
    }

    [Fact]
    public async Task Pull_TransferTest_ShouldPass()
    {
        await TestApplication.ResetQueues();

        var bankTools = new BankTestTool();
        await bankTools.Start();

        await bankTools.BankSecond.AddToBalance(200.00m);
        await bankTools.BankSecond.TestBankBalance(200.00m);

        await bankTools.BankFirst.PullMoney(150.00m, 150.00m, 50.00m);
    }

    [Fact]
    public async Task PushAndPull_TransferTest_ShouldPass()
    {
        await TestApplication.ResetQueues();

        var bankTools = new BankTestTool();
        await bankTools.Start();

        await bankTools.BankSecond.AddToBalance(200.00m);
        await bankTools.BankSecond.TestBankBalance(200.00m);

        await bankTools.BankFirst.MoveMoney(new[] { 150.00m, -50.00m, 20.00m }, 170.00m, 80.00m);
    }
}
