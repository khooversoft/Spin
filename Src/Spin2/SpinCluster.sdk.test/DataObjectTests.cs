using FluentAssertions;
using SpinCluster.sdk.Actors.ActorBase;
using Toolbox.Data;

namespace Toolbox.Test.Data;

public class DataObjectTests
{
    [Fact]
    public void DataObjectSimpleBuilder()
    {
        var d = new DataObjectBuilder()
            .SetKey("key")
            .SetTypeName("type")
            .Add("enable", "true")
            .Build();

        d.Should().NotBeNull();
        d.Key.Should().Be("key");
        d.TypeName.Should().Be("type");
        d.Values.Should().NotBeNull();
        d.Values.Count.Should().Be(1);
        d.Values[0].Key.Should().Be("enable");
        d.Values[0].Value.Should().Be("true");
    }

    [Fact]
    public void DataObjectClassTypeBuilder()
    {
        var t = new TestClass
        {
            Name = "test",
            Value = "value",
        };

        var d = t.ToDataObject();
        d.Should().NotBeNull();
        d.Key.Should().Be("TestClass");
        d.TypeName.Should().Be("TestClass");
        d.Values.Should().NotBeNull();
        d.Values.Count.Should().Be(2);
        d.Values[0].Key.Should().Be("Name");
        d.Values[0].Value.Should().Be("test");
        d.Values[1].Key.Should().Be("Value");
        d.Values[1].Value.Should().Be("value");

        var t2 = d.ToObject<TestClass>();
        (t == t2).Should().BeTrue();
    }

    private record TestClass
    {
        public string Name { get; set; } = null!;
        public string Value { get; set; } = null!;
    }
}
