using AisParser;
using AisParser.Messages;
using FluentAssertions;
using Xunit;

namespace AisParserTests.MessagesTests
{
    public class PositionReportClassAAssignedScheduleMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,B,25Mw@DP000qR9bFA:6KI0AV@00S3,0*0A";

            var message = Parser.Parse(sentence) as PositionReportClassAAssignedScheduleMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.PositionReportClassAAssignedSchedule);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(366989394u);
            message.NavigationStatus.Should().Be(NavigationStatus.UnderWayUsingEngine);
            message.RateOfTurn.Should().Be(0);
            message.SpeedOverGround.Should().Be(0);
            message.PositionAccuracy.Should().Be(PositionAccuracy.High);
            message.Longitude.Should().BeApproximately(-90.40670166666666d, 0.000001d);
            message.Latitude.Should().BeApproximately(29.985461666666666d, 0.000001d);
            message.CourseOverGround.Should().Be(230.5);
            message.TrueHeading.Should().Be(51u);
            message.Timestamp.Should().Be(8u);
            message.ManeuverIndicator.Should().Be(ManeuverIndicator.NotAvailable);
            message.Spare.Should().Be(0u);
            message.Raim.Should().Be(Raim.NotInUse);
            message.RadioStatus.Should().Be(0x8C3u);
        }
    }
}