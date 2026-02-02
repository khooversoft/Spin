using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Data;

public class DataObjectTests
{
    [Fact]
    public void TestSerializerRegistery()
    {
        JsonSerializerContextRegistered.Find<DataObject>().BeOk();
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

        string json = d.ToJson();

        var rd = json.ToObject<DataObject>();

        TestClass rt = rd.ToObject<TestClass>();
        rt.NotNull();
        rt.Name.Be("test");
        rt.Value.Be("value");

        (t == rt).BeTrue();
    }

    [Fact]
    public void DataObjectCreateWithCustomKeyAndTags()
    {
        var t = new TestClass
        {
            Name = "test",
            Value = "value",
        };

        DataObject d = DataObject.Create(t, key: "customKey") with { Tags = "tag1,tag2" };

        d.Key.Be("customKey");
        d.TypeName.Be("TestClass");
        d.Tags.Be("tag1,tag2");

        Option validation = d.Validate();
        validation.IsOk().BeTrue();

        string json = d.ToJson();
        DataObject? roundTrip = json.ToObject<DataObject>();
        roundTrip.NotNull();
        roundTrip!.Tags.Be("tag1,tag2");
        roundTrip.Key.Be("customKey");
    }

    [Fact]
    public void DataObjectValidationShouldFailWhenKeyMissing()
    {
        var invalid = new DataObject
        {
            Key = "",
            TypeName = "TestClass",
            JsonData = "{}",
            CreatedDate = DateTime.UtcNow,
        };

        invalid.Validate().BeError();
    }

    [Fact]
    public void DataObjectCreatedDateDefaultsToUtcNow()
    {
        DateTime before = DateTime.UtcNow;

        DataObject d = DataObject.Create(new TestClass { Name = "test", Value = "value" });

        (d.CreatedDate.Kind == DateTimeKind.Utc).BeTrue();
        (d.CreatedDate >= before && d.CreatedDate <= DateTime.UtcNow).BeTrue();
    }

    [Fact]
    public void DataObjectEqualityRespectsCreatedDate()
    {
        DateTime timestamp = DateTime.UtcNow;

        DataObject a = new DataObject
        {
            Key = "key",
            TypeName = "type",
            JsonData = "{}",
            CreatedDate = timestamp,
        };

        DataObject b = a with { };
        (a == b).BeTrue();
        a.Equals(b).BeTrue();

        DataObject c = a with { CreatedDate = timestamp.AddSeconds(1) };
        (a == c).BeFalse();
        a.Equals(c).BeFalse();
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
