using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Data;

public class DataObjectTests
{

    [Fact]
    public void EmptyDataObjectSetValidation()
    {
        DataObjectSet model = new DataObjectSet();

        Option v = model.Validate().ToOptionStatus();
        v.IsOk().BeTrue();
    }

    [Fact]
    public void DataObjectClassTypeBuilder()
    {
        var t = new TestClass
        {
            Name = "test",
            Value = "value",
        };

        DataObject d = DataObject.Create(t);
        d.NotNull();
        d.Key.Be("TestClass");
        d.TypeName.Be("TestClass");
        d.JsonData.NotEmpty();

        TestClass rt = d.ToObject<TestClass>();
        rt.NotNull();
        rt.Name.Be("test");
        rt.Value.Be("value");

        (t == rt).BeTrue();
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

        DataObjectSet set = new DataObjectSet().Set(t1).Set(t2);

        set.Count.Be(2);

        Option<TestClass> rt1 = set.GetObject<TestClass>();
        rt1.IsOk().BeTrue();
        rt1.Return().Name.Be("test");
        rt1.Return().Value.Be("value");

        Option<TestClass2> rt2 = set.GetObject<TestClass2>();
        rt2.IsOk().BeTrue();
        rt2.Return().Id.Be("id");
        rt2.Return().Description.Be("description");

        string json = set.ToJson();

        DataObjectSet? read = json.ToObject<DataObjectSet>();
        read.NotNull();
        read!.Count.Be(2);

        Option<TestClass> r_rt1 = read.GetObject<TestClass>();
        r_rt1.IsOk().BeTrue();
        r_rt1.Return().Name.Be("test");
        r_rt1.Return().Value.Be("value");

        Option<TestClass2> r_rt2 = read.GetObject<TestClass2>();
        r_rt2.IsOk().BeTrue();
        r_rt2.Return().Id.Be("id");
        r_rt2.Return().Description.Be("description");
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
