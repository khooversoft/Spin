using AisParser;
using AisParser.Messages;
using FluentAssertions;
using Xunit;

namespace AisParserTests.MessagesTests
{
    public class PositionReportClassAResponseToInterrogationMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message_libais_16()
        {
            const string sentence = "!AIVDM,1,1,,B,35MC>W@01EIAn5VA4l`N2;>0015@,0*01";

            var message = Parser.Parse(sentence) as PositionReportClassAResponseToInterrogationMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.PositionReportClassAResponseToInterrogation);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(366268061u);
            message.NavigationStatus.Should().Be(NavigationStatus.UnderWayUsingEngine);
            message.RateOfTurn.Should().Be(0);
            message.SpeedOverGround.Should().Be(8.5);
            message.PositionAccuracy.Should().Be(PositionAccuracy.Low);
            message.Longitude.Should().BeApproximately(-93.96876833333333d, 0.000001d);
            message.Latitude.Should().BeApproximately(29.841335d, 0.000001d);
            message.CourseOverGround.Should().Be(359.2);
            message.TrueHeading.Should().Be(359u);
            message.Timestamp.Should().Be(0u);
            message.ManeuverIndicator.Should().Be(ManeuverIndicator.NotAvailable);
            message.Spare.Should().Be(0u);
            message.Raim.Should().Be(Raim.NotInUse);
            message.RadioStatus.Should().Be(0x1150u);
        }

        [Fact]
        public void Should_parse_message_libais_18()
        {
            const string sentence = "!AIVDM,1,1,,A,35NBTh0Oh1G@Dt8EiccBuE3n00nQ,0*05";

            var message = Parser.Parse(sentence) as PositionReportClassAResponseToInterrogationMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.PositionReportClassAResponseToInterrogation);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(367305920u);
            message.NavigationStatus.Should().Be(NavigationStatus.UnderWayUsingEngine);
            message.RateOfTurn.Should().Be(127);
            message.SpeedOverGround.Should().Be(0.1);
            message.PositionAccuracy.Should().Be(PositionAccuracy.Low);
            message.Longitude.Should().BeApproximately(-122.26239333333334d, 0.000001d);
            message.Latitude.Should().BeApproximately(38.056821666666664d, 0.000001d);
            message.CourseOverGround.Should().Be(75.7);
            message.TrueHeading.Should().Be(161u);
            message.Timestamp.Should().Be(59u);
            message.ManeuverIndicator.Should().Be(ManeuverIndicator.NotAvailable);
            message.Spare.Should().Be(0u);
            message.Raim.Should().Be(Raim.NotInUse);
            message.RadioStatus.Should().Be(3489u);
        }

        [Fact]
        public void Should_parse_message_libais_20()
        {
            const string sentence = "!AIVDM,1,1,,B,35N0IFP016Jf9rVG8mSB?Acl0Pj0,0*4C";

            var message = Parser.Parse(sentence) as PositionReportClassAResponseToInterrogationMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.PositionReportClassAResponseToInterrogation);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(367008090u);
            message.NavigationStatus.Should().Be(NavigationStatus.UnderWayUsingEngine);
            message.RateOfTurn.Should().Be(0);
            message.SpeedOverGround.Should().Be(7);
            message.PositionAccuracy.Should().Be(PositionAccuracy.Low);
            message.Longitude.Should().BeApproximately(-73.80338166666667d, 0.000001d);
            message.Latitude.Should().BeApproximately(40.436715d, 0.000001d);
            message.CourseOverGround.Should().Be(57.3);
            message.TrueHeading.Should().Be(53u);
            message.Timestamp.Should().Be(58u);
            message.ManeuverIndicator.Should().Be(ManeuverIndicator.NotAvailable);
            message.Spare.Should().Be(0u);
            message.Raim.Should().Be(Raim.NotInUse);
            message.RadioStatus.Should().Be(134272u);
        }
    }
}