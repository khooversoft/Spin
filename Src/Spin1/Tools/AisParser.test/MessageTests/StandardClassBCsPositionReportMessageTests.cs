using AisParser;
using AisParser.Messages;
using FluentAssertions;
using Xunit;

namespace AisParserTests.MessagesTests
{
    public class StandardClassBCsPositionReportMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,B,B5NLCa000>fdwc63f?aBKwPUoP06,0*15";

            var message = Parser.Parse(sentence) as StandardClassBCsPositionReportMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.StandardClassBCsPositionReport);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(367465380u);
            message.SpeedOverGround.Should().Be(0u);
            message.PositionAccuracy.Should().Be(PositionAccuracy.High);
            message.Longitude.Should().BeApproximately(-71.03836333333334d, 0.000001d);
            message.Latitude.Should().BeApproximately(42.34964333333333d, 0.000001d);
            message.CourseOverGround.Should().Be(131.8);
            message.TrueHeading.Should().BeNull();
            message.Timestamp.Should().Be(1u);
            message.IsCsUnit.Should().BeTrue();
            message.HasDisplay.Should().BeFalse();
            message.HasDscCapability.Should().BeTrue();
            message.Band.Should().BeTrue();
            message.CanAcceptMessage22.Should().BeTrue();
            message.Assigned.Should().BeFalse();
            message.Raim.Should().Be(Raim.InUse);
            message.RadioStatus.Should().Be(917510u);
        }
    }
}