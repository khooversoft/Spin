using FluentAssertions;
using SpinNet.sdk.Application;
using SpinNet.sdk.Model;
using Toolbox.Extensions;

namespace SpinNet.Test;

public class NetHeaderTests
{
    [Fact]
    public void GivenNoPayload_ShouldBuild()
    {
        const string fromId = "spinSchedule/timer/sched01";
        const string toId = "contract/documentid";

        var message = new NetMessageBuilder()
            .SetFromId(fromId)
            .SetToId(toId)
            .Build();

        message.MessageId.Should().NotBeEmpty();
        message.FromId.Should().Be(fromId);
        message.ResourceUri.Should().Be(toId);
        message.Payloads.Should().HaveCount(0);
    }

    [Fact]
    public void GivenSinglePayload_ShouldBuild()
    {
        const string fromId = "spinSchedule/timer/sched01";
        const string toId = "contract/documentid";
        var payload1 = new Payload1("name1", "value1");

        var payload = new PayloadBuilder()
            .SetContent(payload1)
            .Build();

        var message = new NetMessageBuilder()
            .SetFromId(fromId)
            .SetToId(toId)
            .Add(payload)
            .Build();

        message.MessageId.Should().NotBeEmpty();
        message.FromId.Should().Be(fromId);
        message.ResourceUri.Should().Be(toId);
        message.Payloads.Should().HaveCount(1);

        message.Payloads[0].TypeName.Should().Be(payload1.GetType().GetTypeName());
        message.Payloads[0].Data.Should().Be(payload1.ToJson());

        Payload1 p1 = message.GetTypedPayloads<Payload1>().Single();
        (payload1 == p1).Should().BeTrue();
    }

    private record Payload1(string name, string value);
}