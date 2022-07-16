using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Block.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;
using Xunit;

namespace Toolbox.Block.Test.Serialzation;

public class BlockSerializationTests
{
    [Fact]
    public void WhenSerializeSimpleClass_ShouldPass()
    {
        var a = new ClassA
        {
            Name = "Name",
            Value = "Value",
        };

        IReadOnlyList<DataItem> result = BlockSerializer.Serialize(a);
        result.Should().NotBeNull();
        result.Count.Should().Be(3);

        Test(result, "Id", a.Id.ToString());
        Test(result, "Name", a.Name.ToString());
        Test(result, "Value", a.Value.ToString());

        var newA = BlockSerializer.Deserialize<ClassA>(result);
        newA.Should().NotBeNull();
        newA.Id.Should().Be(a.Id);
        newA.Name.Should().Be(a.Name);
        newA.Value.Should().Be(a.Value);
    }

    [Fact]
    public void WhenSerializeSimpleRecord_ShouldPass()
    {
        var a = new RecordA
        {
            Name = "Name",
            Value = "Value",
        };

        IReadOnlyList<DataItem> result = BlockSerializer.Serialize(a);
        result.Should().NotBeNull();
        result.Count.Should().Be(3);

        Test(result, "Id", a.Id.ToString());
        Test(result, "Name", a.Name.ToString());
        Test(result, "Value", a.Value.ToString());

        var newA = BlockSerializer.Deserialize<RecordA>(result);
        newA.Should().NotBeNull();
        newA.Id.Should().Be(a.Id);
        newA.Name.Should().Be(a.Name);
        newA.Value.Should().Be(a.Value);
        (a == newA).Should().BeTrue();
    }

    [Fact]
    public void WhenSimpleClassIsAddedToDocument_RoundTrip_ShouldPass()
    {
        var document = new BlockDocument();

        var a = new ClassA
        {
            Name = "Name",
            Value = "Value",
        };

        document.Add(a);

        Test(document, "Id", a.Id.ToString());
        Test(document, "Name", a.Name.ToString());
        Test(document, "Value", a.Value.ToString());

        var currentState = document.CurrentState();

        Test(currentState, "Id", a.Id.ToString());
        Test(currentState, "Name", a.Name.ToString());
        Test(currentState, "Value", a.Value.ToString());

        var newA = BlockSerializer.Deserialize<ClassA>(currentState);
        newA.Should().NotBeNull();
        newA.Id.Should().Be(a.Id);
        newA.Name.Should().Be(a.Name);
        newA.Value.Should().Be(a.Value);
    }

    [Fact]
    public void When2SimpleClassIsAddedToDocument_RoundTrip_ShouldPass()
    {
        var document = new BlockDocument();

        var a = new RecordA
        {
            Name = "Name",
            Value = "Value",
        };

        document.Add(a);
        document.CommitDataItems.Count.Should().Be(0);
        document.DataItems.Count.Should().Be(3);

        a = a with { Value = "Value1" };
        document.Add(a);
        document.CommitDataItems.Count.Should().Be(0);
        document.DataItems.Count.Should().Be(4);

        var currentState = document.CurrentState();

        Test(currentState, "Id", a.Id.ToString());
        Test(currentState, "Name", a.Name.ToString());
        Test(currentState, "Value", a.Value.ToString());

        var newA = BlockSerializer.Deserialize<ClassA>(currentState);
        newA.Should().NotBeNull();
        newA.Id.Should().Be(a.Id);
        newA.Name.Should().Be(a.Name);
        newA.Value.Should().Be(a.Value);
    }

    private void Test(IEnumerable<DataItem> dataItems, string key, string value)
    {
        dataItems
            .Where(x => x.Key == key)
            .FirstOrDefault()
            .NotNull(name: "No key")
            .Value.Should().Be(value);
    }


    private class ClassA
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string Value { get; set; } = null!;
    }

    private record RecordA
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string Value { get; set; } = null!;
    }
}
