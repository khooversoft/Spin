using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smart_Installment.sdk.Test;

public class LedgerRecordTests
{
    [Fact]
    public void GivenNormalLedger_WhenEqual_ShouldPass()
    {
        DateTime now = DateTime.UtcNow;
        Guid Id = Guid.NewGuid();

        var ledger1 = new LedgerRecord
        {
            Id = Id,
            Date = now,
            Type = LedgerType.Debit,
            TrxType = "add",
            Amount = 10.55m,
        };

        var ledger2 = new LedgerRecord
        {
            Id = Id,
            Date = now,
            Type = LedgerType.Debit,
            TrxType = "add",
            Amount = 10.55m,
        };

        (ledger1 == ledger2).Should().BeTrue();
    }

    [Fact]
    public void GivenNormalLedgerDifferent_WhenNotEqual_ShouldPass()
    {
        DateTime now = DateTime.UtcNow;
        Guid Id = Guid.NewGuid();

        var ledger1 = new LedgerRecord
        {
            Id = Id,
            Date = now,
            Type = LedgerType.Debit,
            TrxType = "add",
            Amount = 10.54m, // Different
        };

        var ledger2 = new LedgerRecord
        {
            Id = Id,
            Date = now,
            Type = LedgerType.Debit,
            TrxType = "add",
            Amount = 10.55m,
        };

        (ledger1 != ledger2).Should().BeTrue();
    }

    [Fact]
    public void GivenNormalLedgerWithProperties_WhenEqual_ShouldPass()
    {
        DateTime now = DateTime.UtcNow;
        Guid Id = Guid.NewGuid();

        var ledger1 = new LedgerRecord
        {
            Id = Id,
            Date = now,
            Type = LedgerType.Debit,
            TrxType = "add",
            Amount = 10.55m,
            Properties = new[] { "Property1=Value1" }.ToList(),
        };

        var ledger2 = new LedgerRecord
        {
            Id = Id,
            Date = now,
            Type = LedgerType.Debit,
            TrxType = "add",
            Amount = 10.55m,
            Properties = new[] { "Property1=Value1" }.ToList(),
        };

        (ledger1 == ledger2).Should().BeTrue();
    }

    [Fact]
    public void GivenNormalLedgerWithNotLikedProperties_WhenNotEqual_ShouldPass()
    {
        DateTime now = DateTime.UtcNow;
        Guid Id = Guid.NewGuid();

        var ledger1 = new LedgerRecord
        {
            Id = Id,
            Date = now,
            Type = LedgerType.Debit,
            TrxType = "add",
            Amount = 10.55m,
            Properties = new[] { "Property1=Value1" }.ToList(),
        };

        var ledger2 = new LedgerRecord
        {
            Id = Id,
            Date = now,
            Type = LedgerType.Debit,
            TrxType = "add",
            Amount = 10.55m,
            Properties = new[] { "Property1=Value1", "Property2=Value2" }.ToList(),
        };

        (ledger1 != ledger2).Should().BeTrue();
    }
}
