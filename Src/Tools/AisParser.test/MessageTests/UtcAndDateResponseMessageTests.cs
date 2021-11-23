using AisParser;
using AisParser.Messages;
using FluentAssertions;
using Xunit;

namespace AisParserTests.MessagesTests
{
    public class UtcAndDateResponseMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,A,;3UQhR1v<s6kS00DW4Lqw@Q00000,0*5A";

            var message = Parser.Parse(sentence) as UtcAndDateResponseMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.UtcAndDateResponse);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(240677000u);
            message.Year.Should().Be(2019u);
            message.Month.Should().Be(3u);
            message.Day.Should().Be(22u);
            message.Hour.Should().Be(6u);
            message.Minute.Should().Be(51u);
            message.Second.Should().Be(35u);
            message.PositionAccuracy.Should().Be(PositionAccuracy.Low);
            message.Longitude.Should().BeApproximately(	0.07035d, 0.000001d);
            message.Latitude.Should().BeApproximately(	50.517017d, 0.000001d);
            message.PositionFixType.Should().Be(PositionFixType.Gps);
            message.Spare.Should().Be(0u);
            message.Raim.Should().Be(Raim.NotInUse);
            message.RadioStatus.Should().Be(0u);
        }
    }
}