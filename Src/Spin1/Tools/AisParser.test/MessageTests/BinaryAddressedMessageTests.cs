using AisParser;
using AisParser.Messages;
using FluentAssertions;
using Xunit;

namespace AisParserTests.MessagesTests
{
    public class BinaryAddressedMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,A,6>h8nIT00000>d`vP000@00,2*53";

            var message = Parser.Parse(sentence) as BinaryAddressedMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.BinaryAddressedMessage);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(990000742u);
            message.SequenceNumber.Should().Be(1u);
            message.DestinationMmsi.Should().Be(0u);
            message.RetransmitFlag.Should().BeFalse();
            message.DesignatedAreaCode.Should().Be(235u);
            message.FunctionalId.Should().Be(10u);
            message.Data.Should().Be("O(D");
        }
    }
}