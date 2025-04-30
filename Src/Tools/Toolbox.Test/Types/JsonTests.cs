using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Test.Types;

public class JsonTests
{

    [Fact]
    public void ExpandJsonObjectShouldPass()
    {
        DateTime now = DateTime.Now;

        var payload = new
        {
            ClassTypeName = "classType",
            Index = (int)100,
            SubPayload = "this is a test",
        };

        var subPayload = new Payload
        {
            Name = "Name1",
            IntValue = 5,
            DateValue = now,
            FloatValue = 1.5f,
            DecimalValue = 55.23m,
        };

        string sourceJson = payload.ToJson();
        string nodeName = "subPayload";
        string nodeJson = subPayload.ToJson();

        string result = Json.ExpandNode(sourceJson, nodeName, nodeJson);

        var expectedObject = new
        {
            ClassTypeName = "classType",
            Index = (int)100,
            SubPayload = new Payload
            {
                Name = "Name1",
                IntValue = 5,
                DateValue = now,
                FloatValue = 1.5f,
                DecimalValue = 55.23m,
            }
        };

        string expectedJson = expectedObject.ToJson();
        result.Be(expectedJson);
    }

    [Fact]
    public void WrapJsonObject_ShouldPass()
    {
        DateTime now = DateTime.Now;

        var payload = new
        {
            ClassTypeName = "classType",
            Index = (int)100,
            SubPayload = new
            {
                Name = "Name1",
                IntValue = 5,
                DateValue = now,
                FloatValue = 1.5f,
                DecimalValue = 55.23m,
                sub2 = new
                {
                    InnerValue = "inner-value",
                    LogName = "logName",
                    intValue = 2,
                }
            }
        };

        string sourceJson = payload.ToJson();

        string result = Json.WrapNode(sourceJson, "subPayload");

        var expectedPayload = new
        {
            Name = "Name1",
            IntValue = 5,
            DateValue = now,
            FloatValue = 1.5f,
            DecimalValue = 55.23m,
            sub2 = new
            {
                InnerValue = "inner-value",
                LogName = "logName",
                intValue = 2,
            }
        };

        string expectedPayloadJson = expectedPayload.ToJson();

        var expected = new
        {
            ClassTypeName = "classType",
            Index = (int)100,
            SubPayload = expectedPayloadJson,
        };

        string expectedObject = expected.ToJson();
        result.Be(expectedObject);
    }
    private record Payload
    {
        public string Name { get; init; } = null!;
        public int IntValue { get; init; }
        public DateTime DateValue { get; init; }
        public float FloatValue { get; init; }
        public decimal DecimalValue { get; init; }
    }
}
