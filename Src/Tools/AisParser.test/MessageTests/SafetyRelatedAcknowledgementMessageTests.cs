using AisParser;
using AisParser.Messages;
using FluentAssertions;
using Xunit;

namespace AisParserTests.MessagesTests
{
    public class SafetyRelatedAcknowledgementMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,B,=;ISq@BnHvD8,0*66";

            var message = Parser.Parse(sentence) as SafetyRelatedAcknowledgementMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.SafetyRelatedAcknowledgement);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(765000001u);
            message.Spare.Should().Be(0u);
            message.Mmsi1.Should().Be(765000002u);
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