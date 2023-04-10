using AisParser;
using AisParser.Messages;
using FluentAssertions;
using Xunit;

namespace AisParserTests.MessagesTests
{
    public class BinaryAcknowledgeMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,A,75Mu6d0P17IP?PfGSC29WOvb0<14,0*61";

            var message = Parser.Parse(sentence) as BinaryAcknowledgeMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.BinaryAcknowledge);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(366954160u);
            message.Spare.Should().Be(0u);
            message.Mmsi1.Should().Be(134290840u);
            message.SequenceNumber1.Should().Be(0u);
            message.Mmsi2.Should().Be(260236771u);
            message.SequenceNumber2.Should().Be(1u);
            message.Mmsi3.Should().Be(203581311u);
            message.SequenceNumber3.Should().Be(3u);
            message.Mmsi4.Should().Be(713043985u);
            message.SequenceNumber4.Should().Be(0u);
        }

        [Fact]
        public void Should_parse_message_20190212_654382()
        {
            const string sentence = "!AIVDM,1,1,,A,702:oP3dTnnp,0*65";

            var message = Parser.Parse(sentence) as BinaryAcknowledgeMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.BinaryAcknowledge);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(2275200u);
            message.Spare.Should().Be(0u);
            message.Mmsi1.Should().Be(992271214u);
            message.SequenceNumber1.Should().Be(0u);
            message.Mmsi2.Should().BeNull();
            message.SequenceNumber2.Should().Be(0u);
            message.Mmsi3.Should().BeNull();
            message.SequenceNumber3.Should().Be(0u);
            message.Mmsi4.Should().BeNull();
            message.SequenceNumber4.Should().Be(0u);
        }
    }
}