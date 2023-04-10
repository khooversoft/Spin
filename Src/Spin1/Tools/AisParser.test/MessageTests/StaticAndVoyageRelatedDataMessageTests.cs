using AisParser;
using AisParser.Messages;
using FluentAssertions;
using Xunit;

namespace AisParserTests.MessagesTests
{
    public class StaticAndVoyageRelatedDataMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence1 = "!AIVDM,2,1,1,B,53P<GC`000038D5c>01LThi=E10iV2222222220m1P834v2@044kmE20CD53,0*25";
            const string sentence2 = "!AIVDM,2,2,1,B,k`888000000,2*25";

            Parser.Parse(sentence1).Should().BeNull();
            var message = Parser.Parse(sentence2) as StaticAndVoyageRelatedDataMessage;

            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.StaticAndVoyageRelatedData);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(235083598u);
            message.AisVersion.Should().Be(2u);
            message.ImoNumber.Should().Be(0u);
            message.CallSign.Should().Be("2EAZ3");
            message.ShipName.Should().Be("WILLSUPPLY");
            message.ShipType.Should().Be(ShipType.PortTender);
            message.DimensionToBow.Should().Be(12u);
            message.DimensionToStern.Should().Be(8u);
            message.DimensionToPort.Should().Be(3u);
            message.DimensionToStarboard.Should().Be(4u);
            message.PositionFixType.Should().Be(PositionFixType.Undefined2);
            message.EtaMonth.Should().Be(8u);
            message.EtaDay.Should().Be(4u);
            message.EtaHour.Should().Be(16u);
            message.EtaMinute.Should().Be(0u);
            message.Draught.Should().Be(1.6d);
            message.Destination.Should().Be("SOUTHAMPTON");
            message.DataTerminalReady.Should().BeTrue();
            message.Spare.Should().Be(0u);
        }

        [Fact]
        public void Should_parse_another_message()
        {
            const string sentence1 = "!AIVDM,2,1,0,A,58wt8Ui`g??r21`7S=:22058<v05Htp000000015>8OA;0sk,0*7B";
            const string sentence2 = "!AIVDM,2,2,0,A,eQ8823mDm3kP00000000000,2*5D";

            Parser.Parse(sentence1).Should().BeNull();
            var message = Parser.Parse(sentence2) as StaticAndVoyageRelatedDataMessage;

            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.StaticAndVoyageRelatedData);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(603916439u);
            message.AisVersion.Should().Be(0u);
            message.ImoNumber.Should().Be(439303422u);
            message.CallSign.Should().Be("ZA83R");
            message.ShipName.Should().Be("ARCO AVON");
            message.ShipType.Should().Be(ShipType.PassengerNoAdditionalInformation);
            message.DimensionToBow.Should().Be(113u);
            message.DimensionToStern.Should().Be(31u);
            message.DimensionToPort.Should().Be(17u);
            message.DimensionToStarboard.Should().Be(11u);
            message.PositionFixType.Should().Be(PositionFixType.Undefined1);
            message.EtaMonth.Should().Be(3u);
            message.EtaDay.Should().Be(23u);
            message.EtaHour.Should().Be(19u);
            message.EtaMinute.Should().Be(45u);
            message.Draught.Should().Be(13.2d);
            message.Destination.Should().Be("HOUSTON");
            message.DataTerminalReady.Should().BeTrue();
            message.Spare.Should().Be(0u);
        }

        [Fact]
        public void Should_parse_incomplete_message()
        {
            const string sentence = "!AIVDM,1,1,,A,5815AE82DP=uKLPkT004j0l5<Q84800000000017AcS?T0,4*63";

            var message = Parser.Parse(sentence) as StaticAndVoyageRelatedDataMessage;

            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.StaticAndVoyageRelatedData);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(538005844u);
            message.AisVersion.Should().Be(2u);
            message.ImoNumber.Should().Be(9732319u);
            message.CallSign.Should().Be("V7HL9");
            message.ShipName.Should().Be("AL MASHRAB");
            message.ShipType.Should().Be(ShipType.CargoHazardousCategoryA);
            message.DimensionToBow.Should().Be(141u);
            message.DimensionToStern.Should().Be(227u);
            message.DimensionToPort.Should().Be(15u);
            message.DimensionToStarboard.Should().Be(36u);
            message.PositionFixType.Should().Be(PositionFixType.Undefined1);
            message.EtaMonth.Should().Be(0u);
            message.EtaDay.Should().Be(0u);
            message.EtaHour.Should().Be(0u);
            message.EtaMinute.Should().Be(0u);
            message.Draught.Should().Be(0d);
            message.Destination.Should().BeEmpty();
            message.DataTerminalReady.Should().BeTrue();
            message.Spare.Should().Be(0u);
        }

        [Fact]
        public void Should_parse_message_with_nil_eta_1()
        {
            const string sentence1 = "!AIVDM,2,1,6,A,539`vQ400000@SGKGP0P4q<D5@000000000000150@@23t0Ht0B0C@UDQh00,0*6B";
            const string sentence2 = "!AIVDM,2,2,6,A,00000000000,2*22";

            Parser.Parse(sentence1).Should().BeNull();
            var message = Parser.Parse(sentence2) as StaticAndVoyageRelatedDataMessage;

            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.StaticAndVoyageRelatedData);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(211435140u);
            message.AisVersion.Should().Be(1u);
            message.ImoNumber.Should().Be(0u);
            message.CallSign.Should().Be("DH5658");
            message.ShipName.Should().Be("HANSEAT");
            message.ShipType.Should().Be(ShipType.PassengerNoAdditionalInformation);
            message.DimensionToBow.Should().Be(2u);
            message.DimensionToStern.Should().Be(16u);
            message.DimensionToPort.Should().Be(2u);
            message.DimensionToStarboard.Should().Be(3u);
            message.PositionFixType.Should().Be(PositionFixType.Undefined2);
            message.EtaMonth.Should().Be(0u);
            message.EtaDay.Should().Be(0u);
            message.EtaHour.Should().Be(24u);
            message.EtaMinute.Should().Be(60u);
            message.Draught.Should().Be(0.1d);
            message.Destination.Should().Be("HAMBURG");
            message.DataTerminalReady.Should().BeTrue();
            message.Spare.Should().Be(0u);
        }

        [Fact]
        public void Should_parse_message_with_nil_eta_2()
        {
            const string sentence1 = "!AIVDM,2,1,8,A,539I@g400000@;W3;B0@EA@lE:1@4pf3G5v0001I9P963t000011@TUL<><<,0*0B";
            const string sentence2 = "!AIVDM,2,2,8,A,13hjn<<<==@,2*0E";

            Parser.Parse(sentence1).Should().BeNull();
            var message = Parser.Parse(sentence2) as StaticAndVoyageRelatedDataMessage;

            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.StaticAndVoyageRelatedData);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(211177660u);
            message.AisVersion.Should().Be(1u);
            message.ImoNumber.Should().Be(0u);
            message.CallSign.Should().Be("DB9024");
            message.ShipName.Should().Be("DETTMER TANK 51_");
            message.ShipType.Should().Be(ShipType.TankerNoAdditionalInformation);
            message.DimensionToBow.Should().Be(76u);
            message.DimensionToStern.Should().Be(9u);
            message.DimensionToPort.Should().Be(6u);
            message.DimensionToStarboard.Should().Be(3u);
            message.PositionFixType.Should().Be(PositionFixType.Undefined2);
            message.EtaMonth.Should().Be(0u);
            message.EtaDay.Should().Be(0u);
            message.EtaHour.Should().Be(0u);
            message.EtaMinute.Should().Be(0u);
            message.Draught.Should().Be(0d);
            message.Destination.Should().Be("DEBRU00800DOCKX00045");
            message.DataTerminalReady.Should().BeTrue();
            message.Spare.Should().Be(0u);
        }

        [Fact]
        public void Should_parse_message_libais_30()
        {
            const string sentence1 = "!AIVDM,2,1,3,B,55NBjP01mtGIL@CW;SM<D60P5Ld000000000000P0`<3557l0<50@kk@,0*66";
            const string sentence2 = "!AIVDM,2,2,3,B,K5h@00000000000,2*72";

            Parser.Parse(sentence1).Should().BeNull();
            var message = Parser.Parse(sentence2) as StaticAndVoyageRelatedDataMessage;

            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.StaticAndVoyageRelatedData);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(367309440u);
            message.AisVersion.Should().Be(0u);
            message.ImoNumber.Should().Be(7729526u);
            message.CallSign.Should().Be("WDD9287");
            message.ShipName.Should().Be("SEA HAWK");
            message.ShipType.Should().Be(ShipType.TowingLarge);
            message.DimensionToBow.Should().Be(5u);
            message.DimensionToStern.Should().Be(12u);
            message.DimensionToPort.Should().Be(3u);
            message.DimensionToStarboard.Should().Be(5u);
            message.PositionFixType.Should().Be(PositionFixType.Gps);
            message.EtaMonth.Should().Be(4u);
            message.EtaDay.Should().Be(15u);
            message.EtaHour.Should().Be(20u);
            message.EtaMinute.Should().Be(0u);
            message.Draught.Should().Be(4.8d);
            message.Destination.Should().Be("TACOMA,WA");
            message.DataTerminalReady.Should().BeTrue();
            message.Spare.Should().Be(0u);
        }

        [Fact]
        public void Should_parse_message_libais_32()
        {
            const string sentence1 = "!AIVDM,2,1,1,A,55>u@H02;lGc<Ha;L0084i<7GR22222222222216:PE885AU0A4l13H13kBC,0*3D";
            const string sentence2 = "!AIVDM,2,2,1,A,R@hC`4QD;`0,2*06";

            Parser.Parse(sentence1).Should().BeNull();
            var message = Parser.Parse(sentence2) as StaticAndVoyageRelatedDataMessage;

            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.StaticAndVoyageRelatedData);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(351228000u);
            message.AisVersion.Should().Be(0u);
            message.ImoNumber.Should().Be(9163130u);
            message.CallSign.Should().Be("3FJR7");
            message.ShipName.Should().Be("BALSA58");
            message.ShipType.Should().Be(ShipType.Cargo);
            message.DimensionToBow.Should().Be(84u);
            message.DimensionToStern.Should().Be(21u);
            message.DimensionToPort.Should().Be(8u);
            message.DimensionToStarboard.Should().Be(8u);
            message.PositionFixType.Should().Be(PositionFixType.Gps);
            message.EtaMonth.Should().Be(5u);
            message.EtaDay.Should().Be(3u);
            message.EtaHour.Should().Be(5u);
            message.EtaMinute.Should().Be(0u);
            message.Draught.Should().Be(6.8d);
            message.Destination.Should().Be("SPDM DOMINICAN REP.");
            message.DataTerminalReady.Should().BeTrue();
            message.Spare.Should().Be(0u);
        }
    }
}