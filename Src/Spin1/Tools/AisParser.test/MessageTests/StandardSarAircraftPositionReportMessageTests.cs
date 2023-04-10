using AisParser;
using AisParser.Messages;
using FluentAssertions;
using Xunit;

namespace AisParserTests.MessagesTests
{
    public class StandardSarAircraftPositionReportMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,A,90003uhWAcIJe8B;5>rk1D@200Sk,0*7E";

            var message = Parser.Parse(sentence) as StandardSarAircraftPositionReportMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.StandardSarAircraftPositionReport);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(000001015u);
            message.Altitude.Should().Be(157u);
            message.SpeedOverGround.Should().Be(107u);
            message.PositionAccuracy.Should().Be(PositionAccuracy.Low);
            message.Longitude.Should().BeApproximately(-92.033265d, 0.000001d);
            message.Latitude.Should().BeApproximately(19.366791d, 0.000001d);
            message.CourseOverGround.Should().Be(77.3);
            message.Timestamp.Should().Be(17u);
            message.DataTerminalReady.Should().BeFalse();
            message.Spare.Should().Be(0u);
            message.Assigned.Should().BeFalse();
            message.Raim.Should().Be(Raim.NotInUse);
            message.RadioStatus.Should().Be(0x8f3u);
        }
    }
}