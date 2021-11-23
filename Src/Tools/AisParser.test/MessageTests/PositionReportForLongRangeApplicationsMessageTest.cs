using AisParser;
using AisParser.Messages;
using FluentAssertions;
using Xunit;

namespace AisParserTests.MessagesTests
{
    public class PositionReportForLongRangeApplicationsMessageTest : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,B,K77U@48?vMSdNWh@,0*75";
            
            var message = Parser.Parse(sentence) as PositionReportForLongRangeApplicationsMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.PositionReportForLongRangeApplications);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(477712400u);
            message.PositionAccuracy.Should().Be(PositionAccuracy.High);
            message.Raim.Should().Be(Raim.NotInUse);
            message.NavigationStatus.Should().Be(NavigationStatus.UnderWayUsingEngine);
            message.Longitude.Should().BeApproximately(-0.656666d, 0.000001d);
            message.Latitude.Should().BeApproximately(50.448334d, 0.000001d);
            message.SpeedOverGround.Should().Be(15.0);
            message.CourseOverGround.Should().Be(260);
            message.GnssPositionStatus.Should().Be(GnssPositionStatus.CurrentGnssPosition);
            message.Spare.Should().Be(0u);
        }
    }
}