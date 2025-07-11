using SoftBank.sdk.Models;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace SpinClusterApi.test.Models;

public class LedgerItemModelTests
{
    [Fact]
    public void TestLedgerItemValidation()
    {
        var model = new SbLedgerItem
        {
            AccountId = "softbank:company9.com/account1",
            OwnerId = "user1@company9.com",
            Description = "Ledger 1",
            Type = SbLedgerType.Credit,
            Amount = 100.0m
        };

        var v = model.Validate();
        v.IsOk().Should().BeTrue(v.Error);
    }
}
