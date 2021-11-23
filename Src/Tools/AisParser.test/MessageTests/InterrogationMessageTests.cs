using AisParser;
using AisParser.Messages;
using FluentAssertions;
using Xunit;

namespace AisParserTests.MessagesTests
{
    public class InterrogationMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,A,?77U@41:oEPPD00,2*5F";

            var message = Parser.Parse(sentence) as InterrogationMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.Interrogation);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(477712400u);
            message.InterrogatedMmsi.Should().Be(314005000u);
            message.FirstMessageType.Should().Be(AisMessageType.StaticAndVoyageRelatedData);
            message.FirstSlotOffset.Should().Be(0u);
            message.SecondMessageType.Should().BeNull();
            message.SecondSlotOffset.Should().BeNull();
            message.SecondStationInterrogationMmsi.Should().BeNull();
            message.SecondStationFirstMessageType.Should().BeNull();
            message.SecondStationFirstSlotOffset.Should().BeNull();
        }
    }
}