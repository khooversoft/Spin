using AisParser;
using AisParser.Messages;
using FluentAssertions;
using Xunit;

namespace AisParserTests.MessagesTests
{
    public class BaseStationReportMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,A,402MN7iv:HFssOrrk4M4EVw02L1T,0*29";

            var message = Parser.Parse(sentence) as BaseStationReportMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.BaseStationReport);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(2579999u);
            message.Year.Should().Be(2018u);
            message.Month.Should().Be(9u);
            message.Day.Should().Be(16u);
            message.Hour.Should().Be(22u);
            message.Minute.Should().Be(59u);
            message.Second.Should().Be(59u);
            message.PositionAccuracy.Should().Be(PositionAccuracy.Low);
            message.Longitude.Should().BeApproximately(-1.110023d, 0.000001d);
            message.Latitude.Should().BeApproximately(50.799618d, 0.000001d);
            message.PositionFixType.Should().Be(PositionFixType.Undefined2);
            message.Spare.Should().Be(0u);
            message.Raim.Should().Be(Raim.InUse);
            message.RadioStatus.Should().Be(114788u);
        }

        [Fact]
        public void Should_parse_another_message()
        {
            const string sentence = "!AIVDM,1,1,,B,403OK@Quw35W<rsg:hH:wK70087D,0*6E";

            var message = Parser.Parse(sentence) as BaseStationReportMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.BaseStationReport);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(3660610u);
            message.Year.Should().Be(2015u);
            message.Month.Should().Be(12u);
            message.Day.Should().Be(6u);
            message.Hour.Should().Be(5u);
            message.Minute.Should().Be(39u);
            message.Second.Should().Be(12u);
            message.PositionAccuracy.Should().Be(PositionAccuracy.High);
            message.Longitude.Should().BeApproximately(-70.83633333333334d, 0.000001d);
            message.Latitude.Should().BeApproximately(42.24316666666667d, 0.000001d);
            message.PositionFixType.Should().Be(PositionFixType.Surveyed);
            message.Spare.Should().Be(0u);
            message.Raim.Should().Be(Raim.NotInUse);
            message.RadioStatus.Should().Be(33236u);
        }

        [Fact]
        public void Should_parse_message_libais_25()
        {
            const string sentence = "!AIVDM,1,1,,A,402u=TiuaA000r5UJ`H4`?7000S:,0*75";

            var message = Parser.Parse(sentence) as BaseStationReportMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.BaseStationReport);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(3100051u);
            message.Year.Should().Be(2010u);
            message.Month.Should().Be(5u);
            message.Day.Should().Be(2u);
            message.Hour.Should().Be(0u);
            message.Minute.Should().Be(0u);
            message.Second.Should().Be(0u);
            message.PositionAccuracy.Should().Be(PositionAccuracy.High);
            message.Longitude.Should().BeApproximately(-82.6661d, 0.000001d);
            message.Latitude.Should().BeApproximately(42.069433333333336d, 0.000001d);
            message.PositionFixType.Should().Be(PositionFixType.Surveyed);
            message.Spare.Should().Be(0u);
            message.Raim.Should().Be(Raim.NotInUse);
            message.RadioStatus.Should().Be(2250u);
        }

        [Fact]
        public void Should_parse_message_libais_26()
        {
            const string sentence = "!AIVDM,1,1,,A,403OweAuaAGssGWDABBdKBA006sd,0*07";

            var message = Parser.Parse(sentence) as BaseStationReportMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.BaseStationReport);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(3669941u);
            message.Year.Should().Be(2010u);
            message.Month.Should().Be(5u);
            message.Day.Should().Be(2u);
            message.Hour.Should().Be(23u);
            message.Minute.Should().Be(59u);
            message.Second.Should().Be(59u);
            message.PositionAccuracy.Should().Be(PositionAccuracy.Low);
            message.Longitude.Should().BeApproximately(-117.24025166666667d, 0.000001d);
            message.Latitude.Should().BeApproximately(32.670415d, 0.000001d);
            message.PositionFixType.Should().Be(PositionFixType.Gps);
            message.Spare.Should().Be(0u);
            message.Raim.Should().Be(Raim.NotInUse);
            message.RadioStatus.Should().Be(28396u);
        }

        [Fact]
        public void Should_parse_message_libais_27()
        {
            const string sentence = "!AIVDM,1,1,,B,4h3OvjAuaAGsro=cf0Knevo00`S8,0*7E";

            var message = Parser.Parse(sentence) as BaseStationReportMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.BaseStationReport);
            message.Repeat.Should().Be(3u);
            message.Mmsi.Should().Be(3669705u);
            message.Year.Should().Be(2010u);
            message.Month.Should().Be(5u);
            message.Day.Should().Be(2u);
            message.Hour.Should().Be(23u);
            message.Minute.Should().Be(59u);
            message.Second.Should().Be(58u);
            message.PositionAccuracy.Should().Be(PositionAccuracy.High);
            message.Longitude.Should().BeApproximately(-122.84d, 0.000001d);
            message.Latitude.Should().BeApproximately(48.68009833333333d, 0.000001d);
            message.PositionFixType.Should().Be(PositionFixType.Surveyed);
            message.Spare.Should().Be(0u);
            message.Raim.Should().Be(Raim.NotInUse);
            message.RadioStatus.Should().Be(166088u);
        }

        [Fact]
        public void Should_parse_message_20190212_154105()
        {
            const string sentence = "!AIVDM,1,1,,A,402MN7iv<V5r,0*16";

            var message = Parser.Parse(sentence) as BaseStationReportMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.BaseStationReport);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(2579999u);
            message.Year.Should().Be(2019u);
            message.Month.Should().Be(2u);
            message.Day.Should().Be(12u);
            message.Hour.Should().Be(5u);
            message.Minute.Should().Be(58u);
            message.Second.Should().Be(0u);
            message.PositionAccuracy.Should().Be(PositionAccuracy.Low);
            message.Longitude.Should().BeApproximately(0d, 0.000001d);
            message.Latitude.Should().BeApproximately(0d, 0.000001d);
            message.PositionFixType.Should().Be(PositionFixType.Undefined1);
            message.Spare.Should().Be(0u);
            message.Raim.Should().Be(Raim.NotInUse);
            message.RadioStatus.Should().Be(0u);
        }
    }
}