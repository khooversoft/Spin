using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Should;

namespace Toolbox.Test.Tools;

public class JsonSerializationTests
{
    private record TestRecord(string Name, int Age);

    [Fact]
    public void StandardClassSerialization()
    {
        var t1 = new TestRecord("name1", 10);
        string data = t1.ToJson();
        data.Should().NotBeEmpty();
        data.Should().Be("{\"name\":\"name1\",\"age\":10}");

        var t2 = data.ToObject<TestRecord>().NotNull();
        (t1 == t2).Should().BeTrue();
    }

    [Fact]
    public void StandardPascalClassSerialization()
    {
        var t1 = new TestRecord("name1", 10);
        string data = t1.ToJsonPascal();
        data.Should().NotBeEmpty();
        data.Should().Be("{\"Name\":\"name1\",\"Age\":10}");

        var t2 = data.ToObject<TestRecord>().NotNull();
        (t1 == t2).Should().BeTrue();
    }

    [Fact]
    public void StandardFormatClassSerialization()
    {
        var t1 = new TestRecord("name1", 10);
        string data = t1.ToJsonFormat();
        data.Should().NotBeEmpty();
        data.Should().Be("""
            {
              "name": "name1",
              "age": 10
            }
            """);

        var t2 = data.ToObject<TestRecord>().NotNull();
        (t1 == t2).Should().BeTrue();
    }

}
