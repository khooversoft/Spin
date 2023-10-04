using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

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
        d.Values["enable"].Should().Be("true");
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
        d.Values["Name"].Should().Be("test");
        d.Values["Value"].Should().Be("value");

        var t2 = d.ToObject<TestClass>();
        (t == t2).Should().BeTrue();
    }

    [Fact]
    public void DataObjectCollection()
    {
        var t1 = new TestClass
        {
            Name = "test",
            Value = "value",
        };

        var t2 = new TestClass2
        {
            Id = "id",
            Description = "description",
        };

        DataObjectSet set = new DataObjectSetBuilder()
            .Add(t1)
            .Add(t2)
            .Build();

        set.Items.Count.Should().Be(2);

        Option<TestClass> rt1 = set.GetObject<TestClass>();
        rt1.IsOk().Should().BeTrue();
        rt1.Return().Name.Should().Be("test");
        rt1.Return().Value.Should().Be("value");

        Option<TestClass2> rt2 = set.GetObject<TestClass2>();
        rt2.IsOk().Should().BeTrue();
        rt2.Return().Id.Should().Be("id");
        rt2.Return().Description.Should().Be("description");

        string json = set.ToJson();

        DataObjectSet? read = json.ToObject<DataObjectSet>();
        read.Should().NotBeNull();
        read!.Items.Count.Should().Be(2);

        Option<TestClass> r_rt1 = read.GetObject<TestClass>();
        r_rt1.IsOk().Should().BeTrue();
        r_rt1.Return().Name.Should().Be("test");
        r_rt1.Return().Value.Should().Be("value");

        Option<TestClass2> r_rt2 = read.GetObject<TestClass2>();
        r_rt2.IsOk().Should().BeTrue();
        r_rt2.Return().Id.Should().Be("id");
        r_rt2.Return().Description.Should().Be("description");
    }

    private record TestClass
    {
        public string Name { get; set; } = null!;
        public string Value { get; set; } = null!;
    }

    private record TestClass2
    {
        public string Id { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
