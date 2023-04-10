using AisParser;
using AisParser.Messages;
using FluentAssertions;
using Xunit;

namespace AisParserTests.MessagesTests
{
    public class UtcAndDateInquiryMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,B,:5Tjij0qHL8P,0*3A";

            var message = Parser.Parse(sentence) as UtcAndDateInquiryMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.UtcAndDateInquiry);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(374125000u);
            message.Spare1.Should().Be(0u);
            message.DestinationMmsi.Should().Be(240677000u);
            message.Spare2.Should().Be(0u);
        }
    }
}