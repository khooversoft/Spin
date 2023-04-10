using AisParser;
using AisParser.Messages;
using FluentAssertions;
using Xunit;

namespace AisParserTests.MessagesTests
{
    public class AidToNavigationReportMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,B,ENk`sR9`92ah97PR9h0W1T@1@@@=MTpS<7GFP00003vP000,2*4B";

            var message = Parser.Parse(sentence) as AidToNavigationReportMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.AidToNavigationReport);
            message.Repeat.Should().Be(1u);
            message.Mmsi.Should().Be(993672072u);
            message.NavigationalAidType.Should().Be(NavigationalAidType.BeaconSpecialMark);
            message.Name.Should().Be("PRES ROADS ANCH B");
            message.PositionAccuracy.Should().Be(PositionAccuracy.Low);
            message.Longitude.Should().BeApproximately(-70.96399500000001d, 0.000001d);
            message.Latitude.Should().BeApproximately(42.34526d, 0.000001d);
            message.DimensionToBow.Should().Be(0u);
            message.DimensionToStern.Should().Be(0u);
            message.DimensionToPort.Should().Be(0u);
            message.DimensionToStarboard.Should().Be(0u);
            message.PositionFixType.Should().Be(PositionFixType.Surveyed);
            message.Timestamp.Should().Be(61u);
            message.OffPosition.Should().BeFalse();
            message.Raim.Should().Be(Raim.NotInUse);
            message.VirtualAid.Should().BeFalse();
            message.Assigned.Should().BeFalse();
        }

        [Fact]
        public void Should_parse_multipart_message()
        {
            const string sentence1 = "!AIVDM,2,1,5,B,E1c2;q@b44ah4ah0h:2ab@70VRpU<Bgpm4:gP50HH`Th`QF5,0*79";
            const string sentence2 = "!AIVDM,2,2,5,B,1CQ1A83PCAH0,0*62";

            Parser.Parse(sentence1).Should().BeNull();
            var message = Parser.Parse(sentence2) as AidToNavigationReportMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.AidToNavigationReport);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(112233445u);
            message.NavigationalAidType.Should().Be(NavigationalAidType.ReferencePoint);
            message.Name.Should().Be("THIS IS A TEST NAME1");
            message.PositionAccuracy.Should().Be(PositionAccuracy.Low);
            message.Longitude.Should().BeApproximately(145.181d, 0.000001d);
            message.Latitude.Should().BeApproximately(-38.220166666666664d, 0.000001d);
            message.DimensionToBow.Should().Be(5u);
            message.DimensionToStern.Should().Be(3u);
            message.DimensionToPort.Should().Be(3u);
            message.DimensionToStarboard.Should().Be(5u);
            message.PositionFixType.Should().Be(PositionFixType.Gps);
            message.Timestamp.Should().Be(9u);
            message.OffPosition.Should().BeTrue();
            message.Raim.Should().Be(Raim.NotInUse);
            message.VirtualAid.Should().BeFalse();
            message.Assigned.Should().BeTrue();
            message.NameExtension.Should().Be("EXTENDED NAME");
        }

        [Fact]
        public void Should_parse_partial_message()
        {
            const string sentence = "!AIVDM,1,1,,B,E>jHCcAQ90VQ62h84V2h@@@@@@@O,0*21";

            var message = Parser.Parse(sentence) as AidToNavigationReportMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.AidToNavigationReport);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(992351149u);
            message.NavigationalAidType.Should().Be(NavigationalAidType.FixedStuctureOffShore);
            message.Name.Should().Be("BRAMBLE PILE");
            message.PositionAccuracy.Should().Be(PositionAccuracy.High);
        }
    }
}