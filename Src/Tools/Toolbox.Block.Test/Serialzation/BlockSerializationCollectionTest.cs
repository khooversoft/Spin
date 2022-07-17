using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Block.Serialization;
using Toolbox.Tools;
using Xunit;

namespace Toolbox.Block.Test.Serialzation;

public class BlockSerializationCollectionTest
{
    [Fact]
    public void WhenRecordIsAddedToDocument_RoundTrip_ShouldPass()
    {
        var document = new BlockDocument();

        var a = new RecordA
        {
            Name = "Name",
            Value = "Value",
            StringList = Enumerable.Range(0, 3).Select(x => $"Line {x}").ToArray(),
            LedgerItems = new[]
            {
                new Ledger
                {
                    Credit = false,
                    Amount = 100.5m,
                },
                new Ledger
                {
                    Credit = true,
                    Amount = 200.5m,
                },
            }
        };

        document.Add(a);
        document.CommitDataItems.Count.Should().Be(0);
        document.DataItems.Count.Should().Be(14);

        var currentState = document.CurrentState();
        currentState.Count.Should().Be(14);

        var newA = BlockSerializer.Deserialize<RecordA>(currentState);
        newA.Should().NotBeNull();
        newA.Id.Should().Be(a.Id);
        newA.Name.Should().Be(a.Name);
        newA.Value.Should().Be(a.Value);

        newA.StringList.Count().Should().Be(a.StringList.Count());
        newA.StringList.OrderBy(x => x)
            .Zip(a.StringList.OrderBy(x => x))
            .All(x => x.First == x.Second)
            .Should().BeTrue();

        newA.LedgerItems.Count().Should().Be(a.LedgerItems.Count());
        newA.LedgerItems.OrderBy(x => x.Amount)
            .Zip(a.LedgerItems.OrderBy(x => x.Amount))
            .All(x => x.First.Credit == x.Second.Credit && x.First.Amount == x.Second.Amount)
            .Should().BeTrue();
    }

    [Fact]
    public void WhenRecordIsAddedToDocumentWithOneAdd_RoundTrip_ShouldPass()
    {
        var document = new BlockDocument();

        var a = new RecordA
        {
            Name = "Name",
            Value = "Value",
            StringList = Enumerable.Range(0, 3).Select(x => $"Line {x}").ToArray(),
            LedgerItems = new[]
            {
                new Ledger
                {
                    Credit = false,
                    Amount = 100.5m,
                },
                new Ledger
                {
                    Credit = true,
                    Amount = 200.5m,
                },
            }
        };

        document.Add(a);
        document.CommitDataItems.Count.Should().Be(0);
        document.DataItems.Count.Should().Be(14);

        a = a with { StringList = a.StringList.Append("Line 3").ToArray() };
        a = a with { LedgerItems = a.LedgerItems.Append(new Ledger { Amount = 500.33m }).ToArray() };
        document.Add(a);

        var currentState = document.CurrentState();
        currentState.Count.Should().Be(19);

        var newA = BlockSerializer.Deserialize<RecordA>(currentState);
        newA.Should().NotBeNull();
        newA.Id.Should().Be(a.Id);
        newA.Name.Should().Be(a.Name);
        newA.Value.Should().Be(a.Value);

        newA.StringList.Count().Should().Be(a.StringList.Count());
        newA.StringList.OrderBy(x => x)
            .Zip(a.StringList.OrderBy(x => x))
            .All(x => x.First == x.Second)
            .Should().BeTrue();

        newA.LedgerItems.Count().Should().Be(a.LedgerItems.Count());
        newA.LedgerItems.OrderBy(x => x.Amount)
            .Zip(a.LedgerItems.OrderBy(x => x.Amount))
            .All(x => x.First.Credit == x.Second.Credit && x.First.Amount == x.Second.Amount)
            .Should().BeTrue();
    }    


    private void Test(IEnumerable<DataItem> dataItems, string key, string value)
    {
        dataItems
            .Where(x => x.Key == key)
            .FirstOrDefault()
            .NotNull(name: "No key")
            .Value.Should().Be(value);
    }

    private record RecordA
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string Value { get; set; } = null!;
        public string[] StringList { get; init; } = Array.Empty<string>();
        public Ledger[] LedgerItems { get; init; } = Array.Empty<Ledger>();

    }

    public record Ledger
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public DateTimeOffset Date { get; init; } = DateTimeOffset.UtcNow;
        public bool Credit { get; init; }
        public decimal Amount { get; init; }
    }
}
