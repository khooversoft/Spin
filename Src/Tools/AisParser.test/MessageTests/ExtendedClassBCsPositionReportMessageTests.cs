using AisParser;
using AisParser.Messages;
using FluentAssertions;
using Xunit;

namespace AisParserTests.MessagesTests
{
    public class ExtendedClassBCsPositionReportMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,B,C3M@J>00:7vP47WASnqO40N0VPHBa0`@T:;111111110e2t0000P,0*00";

            var message = Parser.Parse(sentence) as ExtendedClassBCsPositionReportMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.ExtendedClassBCsPositionReport);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(232004152u);
            message.Reserved.Should().Be(0u);
            message.SpeedOverGround.Should().Be(4u);
            message.PositionAccuracy.Should().Be(PositionAccuracy.Low);
            message.Longitude.Should().BeApproximately(-1.3098416d, 0.000001d);
            message.Latitude.Should().BeApproximately(50.851597d, 0.000001d);
            message.CourseOverGround.Should().Be(152.1);
            message.TrueHeading.Should().Be(0u);
            message.Timestamp.Should().Be(60u);
            message.RegionalReserved.Should().Be(0u);
            message.Name.Should().Be("SPLIT THREE");
            message.ShipType.Should().Be(ShipType.OtherType);
            message.DimensionToBow.Should().Be(47u);
            message.DimensionToStern.Should().Be(0u);
            message.DimensionToPort.Should().Be(0u);
            message.DimensionToStarboard.Should().Be(0u);
            message.PositionFixType.Should().Be(PositionFixType.Undefined1);
            message.Raim.Should().Be(Raim.NotInUse);
            message.DataTerminalReady.Should().BeFalse();
            message.Assigned.Should().BeFalse();
            message.Spare.Should().Be(0u);
        }
    }
}