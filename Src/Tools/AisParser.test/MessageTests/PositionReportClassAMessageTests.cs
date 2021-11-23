using AisParser;
using AisParser.Messages;
using FluentAssertions;
using Xunit;

namespace AisParserTests.MessagesTests
{
    public class PositionReportClassAMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,B,13GmFd002pwrel@LpMu8L6qn8Vp0,0*56";

            var message = Parser.Parse(sentence) as PositionReportClassAMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.PositionReportClassA);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(226318000u);
            message.NavigationStatus.Should().Be(NavigationStatus.UnderWayUsingEngine);
            message.RateOfTurn.Should().Be(0);
            message.SpeedOverGround.Should().Be(18.4);
            message.PositionAccuracy.Should().Be(PositionAccuracy.High);
            message.Longitude.Should().BeApproximately(-1.154333d, 0.000001d);
            message.Latitude.Should().BeApproximately(50.475500d, 0.000001d);
            message.CourseOverGround.Should().Be(216);
            message.TrueHeading.Should().Be(220u);
            message.Timestamp.Should().Be(59u);
            message.ManeuverIndicator.Should().Be(ManeuverIndicator.NotAvailable);
            message.Spare.Should().Be(2u);
            message.Raim.Should().Be(Raim.NotInUse);
            message.RadioStatus.Should().Be(0x26E00u);
        }

        [Fact]
        public void Should_parse_message_libais_4()
        {
            const string sentence = "!AIVDM,1,1,,A,15B4FT5000JRP>PE6E68Nbkl0PS5,0*70";

            var message = Parser.Parse(sentence) as PositionReportClassAMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.PositionReportClassA);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(354490000u);
            message.NavigationStatus.Should().Be(NavigationStatus.Moored);
            message.RateOfTurn.Should().Be(0);
            message.SpeedOverGround.Should().Be(0);
            message.PositionAccuracy.Should().Be(PositionAccuracy.Low);
            message.Longitude.Should().BeApproximately(-76.34866666666667d, 0.000001d);
            message.Latitude.Should().BeApproximately(36.873d, 0.000001d);
            message.CourseOverGround.Should().Be(217);
            message.TrueHeading.Should().Be(345u);
            message.Timestamp.Should().Be(58u);
            message.ManeuverIndicator.Should().Be(ManeuverIndicator.NotAvailable);
            message.Spare.Should().Be(0u);
            message.Raim.Should().Be(Raim.NotInUse);
            message.RadioStatus.Should().Be(133317u);
        }

        [Fact]
        public void Should_parse_message_libais_6()
        {
            const string sentence = "!AIVDM,1,1,,B,15Mw1U?P00qNGTP@v`0@9wwn26sd,0*0E";

            var message = Parser.Parse(sentence) as PositionReportClassAMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.PositionReportClassA);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(366985620u);
            message.NavigationStatus.Should().Be(NavigationStatus.NotDefined);
            message.RateOfTurn.Should().BeNull();
            message.SpeedOverGround.Should().Be(0);
            message.PositionAccuracy.Should().Be(PositionAccuracy.High);
            message.Longitude.Should().BeApproximately(-91.23304d, 0.000001d);
            message.Latitude.Should().BeApproximately(29.672108333333334d, 0.000001d);
            message.CourseOverGround.Should().Be(3.9);
            message.TrueHeading.Should().BeNull();
            message.Timestamp.Should().Be(59u);
            message.ManeuverIndicator.Should().Be(ManeuverIndicator.NotAvailable);
            message.Spare.Should().Be(0u);
            message.Raim.Should().Be(Raim.InUse);
            message.RadioStatus.Should().Be(28396u);
        }

        [Fact]
        public void Should_parse_message_libais_8()
        {
            const string sentence = "!AIVDM,1,1,,B,15N5s90P00IB>dtA7f<pOwv00<1a,0*2B";

            var message = Parser.Parse(sentence) as PositionReportClassAMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.PositionReportClassA);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(367098660u);
            message.NavigationStatus.Should().Be(NavigationStatus.UnderWayUsingEngine);
            message.RateOfTurn.Should().BeNull();
            message.SpeedOverGround.Should().Be(0);
            message.PositionAccuracy.Should().Be(PositionAccuracy.Low);
            message.Longitude.Should().BeApproximately(-93.88475d, 0.000001d);
            message.Latitude.Should().BeApproximately(29.920511666666666d, 0.000001d);
            message.CourseOverGround.Should().Be(217.5);
            message.TrueHeading.Should().BeNull();
            message.Timestamp.Should().Be(0u);
            message.ManeuverIndicator.Should().Be(ManeuverIndicator.NotAvailable);
            message.Spare.Should().Be(0u);
            message.Raim.Should().Be(Raim.NotInUse);
            message.RadioStatus.Should().Be(49257u);
        }

        [Fact]
        public void Should_parse_message_libais_10()
        {
            const string sentence = "!AIVDM,1,1,,B,15Mq4J0P01EREODRv4@74gv00HRq,0*72";

            var message = Parser.Parse(sentence) as PositionReportClassAMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.PositionReportClassA);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(366888040u);
            message.NavigationStatus.Should().Be(NavigationStatus.UnderWayUsingEngine);
            message.RateOfTurn.Should().BeNull();
            message.SpeedOverGround.Should().Be(0.1);
            message.PositionAccuracy.Should().Be(PositionAccuracy.Low);
            message.Longitude.Should().BeApproximately(-146.29038333333332d, 0.000001d);
            message.Latitude.Should().BeApproximately(61.114133333333335d, 0.000001d);
            message.CourseOverGround.Should().Be(181);
            message.TrueHeading.Should().BeNull();
            message.Timestamp.Should().Be(0u);
            message.ManeuverIndicator.Should().Be(ManeuverIndicator.NotAvailable);
            message.Spare.Should().Be(0u);
            message.Raim.Should().Be(Raim.NotInUse);
            message.RadioStatus.Should().Be(100537u);
        }

        [Fact]
        public void Should_parse_message_with_type_0()
        {
            const string sentence = "!AIVDM,1,1,,B,001vUEEEOP@p2mLWh0nWvd107@jc,0*15";

            var message = Parser.Parse(sentence) as PositionReportClassAMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.PositionReportClassA);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(2073941u);
            message.NavigationStatus.Should().Be(NavigationStatus.Moored);
            message.RateOfTurn.Should().Be(85); // TODO: should this be 322.5 ?
            message.SpeedOverGround.Should().Be(99.2);
            message.PositionAccuracy.Should().Be(PositionAccuracy.Low);
            message.Longitude.Should().BeApproximately(-211.4531500d, 0.000001d);  // TODO: check longitude value
            message.Latitude.Should().BeApproximately(69.4685233d, 0.000001d);
            message.CourseOverGround.Should().Be(204.2);
            message.TrueHeading.Should().Be(384u);
            message.Timestamp.Should().Be(32u);
            message.ManeuverIndicator.Should().Be(ManeuverIndicator.NotAvailable);
            message.Spare.Should().Be(1u);
            message.Raim.Should().Be(Raim.InUse);
            message.RadioStatus.Should().Be(330923u);
        }
    }
}