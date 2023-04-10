using AisParser;
using AisParser.Messages;
using FluentAssertions;
using Xunit;

namespace AisParserTests.MessagesTests
{
    public class StaticDataReportMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_part_A_message()
        {
            const string sentence = "!AIVDM,1,1,,B,H5NLCa0JuJ0U8tr0l4T@Dp00000,2*1C";

            var message = Parser.Parse(sentence) as StaticDataReportPartAMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.StaticDataReport);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(367465380u);
            message.PartNumber.Should().Be(0u);
            message.ShipName.Should().Be("F/V IRON MAIDEN");
        }

        [Fact]
        public void Should_parse_part_B_message()
        {
            const string sentence = "!AIVDM,1,1,,B,H5NLCa4NCD=6mTDG46mnji000000,0*36";

            var message = Parser.Parse(sentence) as StaticDataReportPartBMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.StaticDataReport);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(367465380u);
            message.PartNumber.Should().Be(1u);
            message.ShipType.Should().Be(ShipType.Fishing);
            message.VendorId.Should().Be("STM");
            message.UnitModelCode.Should().Be(1u);
            message.SerialNumber.Should().Be(743700u);
            message.CallSign.Should().Be("WDF5621");
            message.DimensionToBow.Should().Be(0u);
            message.DimensionToStern.Should().Be(0u);
            message.DimensionToPort.Should().Be(0u);
            message.DimensionToStarboard.Should().Be(0u);
            message.PositionFixType.Should().Be(PositionFixType.Undefined1);
            message.Spare.Should().Be(0u);
        }

        [Fact]
        public void Should_parse_another_part_B_message()
        {
            const string sentence = "!AIVDM,1,1,,B,H1c2;qDTijklmno31<<C970`43<1,0*2A";

            var message = Parser.Parse(sentence) as StaticDataReportPartBMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.StaticDataReport);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(112233445u);
            message.PartNumber.Should().Be(1u);
            message.ShipType.Should().Be(ShipType.Sailing);
            message.VendorId.Should().Be("123");
            message.UnitModelCode.Should().Be(13u);
            message.SerialNumber.Should().Be(220599u);
            message.CallSign.Should().Be("CALLSIG");
            message.DimensionToBow.Should().Be(5u);
            message.DimensionToStern.Should().Be(4u);
            message.DimensionToPort.Should().Be(3u);
            message.DimensionToStarboard.Should().Be(12u);
            message.PositionFixType.Should().Be(PositionFixType.Undefined1);
            message.Spare.Should().Be(1u);
        }
    }
}