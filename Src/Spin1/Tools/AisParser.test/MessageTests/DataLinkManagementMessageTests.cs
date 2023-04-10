using AisParser;
using AisParser.Messages;
using FluentAssertions;
using Xunit;

namespace AisParserTests.MessagesTests
{
    public class DataLinkManagementMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,B,D03OK@QclNfp00N007pf9H80v9H,2*33";

            var message = Parser.Parse(sentence) as DataLinkManagementMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.DataLinkManagement);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(3660610u);
            message.Spare.Should().Be(0u);
            message.Offset1.Should().Be(1725u);
            message.ReservedSlots1.Should().Be(1u);
            message.Timeout1.Should().Be(7u);
            message.Increment1.Should().Be(750u);
            message.Offset2.Should().Be(0u);
            message.ReservedSlots2.Should().Be(1u);
            message.Timeout2.Should().Be(7u);
            message.Increment2.Should().Be(0u);
            message.Offset3.Should().Be(126u);
            message.ReservedSlots3.Should().Be(2u);
            message.Timeout3.Should().Be(7u);
            message.Increment3.Should().Be(150u);
            message.Offset4.Should().Be(128u);
            message.ReservedSlots4.Should().Be(3u);
            message.Timeout4.Should().Be(7u);
            message.Increment4.Should().Be(150u);
        }
    }
}
