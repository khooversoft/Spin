using AisParser;
using AisParser.Messages;
using FluentAssertions;
using Xunit;

namespace AisParserTests.MessagesTests
{
    public class AddressedSafetyRelatedMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,A,<5MwpVn0AAup=C7P6B?=Pknnqqqoho0,2*17";

            var message = Parser.Parse(sentence) as AddressedSafetyRelatedMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.AddressedSafetyRelatedMessage);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(366999707u);
            message.SequenceNumber.Should().Be(1u);
            message.DestinationMmsi.Should().Be(538003422u);
            message.RetransmitFlag.Should().BeFalse();
            message.Text.Should().Be("MSG FROM 366999707");
        }
    }
}