using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using SoftBank.sdk.Models;
using Toolbox.Types;

namespace SpinClusterApi.test.Models;

public class LedgerItemModelTests
{
    [Fact]
    public void TestLedgerItemValidation()
    {
        var model = new LedgerItem
        {
            AccountId = "softbank:company9.com/account1",
            OwnerId = "user1@company9.com",
            Description = "Ledger 1",
            Type = LedgerType.Credit,
            Amount = 100.0m
        };

        var v = model.Validate();
        v.IsOk().Should().BeTrue(v.Error);
    }
}
