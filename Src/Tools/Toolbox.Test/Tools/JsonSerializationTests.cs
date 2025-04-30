using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Test.Tools;

public class JsonSerializationTests
{
    private record TestRecord(string Name, int Age);

    [Fact]
    public void StandardClassSerialization()
    {
        var t1 = new TestRecord("name1", 10);
        string data = t1.ToJson();
        data.NotEmpty();
        data.Be("{\"name\":\"name1\",\"age\":10}");

        var t2 = data.ToObject<TestRecord>().NotNull();
        (t1 == t2).BeTrue();
    }

    [Fact]
    public void StandardPascalClassSerialization()
    {
        var t1 = new TestRecord("name1", 10);
        string data = t1.ToJsonPascal();
        data.NotEmpty();
        data.Be("{\"Name\":\"name1\",\"Age\":10}");

        var t2 = data.ToObject<TestRecord>().NotNull();
        (t1 == t2).BeTrue();
    }

    [Fact]
    public void StandardFormatClassSerialization()
    {
        var t1 = new TestRecord("name1", 10);
        string data = t1.ToJsonFormat();
        data.NotEmpty();
        data.Be("""
            {
              "name": "name1",
              "age": 10
            }
            """);

        var t2 = data.ToObject<TestRecord>().NotNull();
        (t1 == t2).BeTrue();
    }

}
