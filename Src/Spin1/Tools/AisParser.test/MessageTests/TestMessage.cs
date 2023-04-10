using AisParser.Messages;
using AisParserTests.MessagesTests;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AisParser.test.MessageTests
{
    public class TestMessage : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,,405flgQv<PP0`24E;QeSC:700L02,0*6F";

            var message = Parser.Parse(sentence);
                
            //    as UtcAndDateResponseMessage;
            //message.Should().NotBeNull();
            //message!.MessageType.Should().Be(AisMessageType.UtcAndDateResponse);
            //message.Repeat.Should().Be(0u);
            //message.Mmsi.Should().Be(240677000u);
            //message.Year.Should().Be(2019u);
            //message.Month.Should().Be(3u);
            //message.Day.Should().Be(22u);
            //message.Hour.Should().Be(6u);
            //message.Minute.Should().Be(51u);
            //message.Second.Should().Be(35u);
            //message.PositionAccuracy.Should().Be(PositionAccuracy.Low);
            //message.Longitude.Should().BeApproximately(0.07035d, 0.000001d);
            //message.Latitude.Should().BeApproximately(50.517017d, 0.000001d);
            //message.PositionFixType.Should().Be(PositionFixType.Gps);
            //message.Spare.Should().Be(0u);
            //message.Raim.Should().Be(Raim.NotInUse);
            //message.RadioStatus.Should().Be(0u);
        }

        [Fact]
        public void Should_parse_message2()
        {
            const string sentence1 = "!AIVDM,2,1,8,,5P000Oh1IT0svTP2r:43grwb05q41P000Oh1IT0svTP2r:43grwb05q41P00,0*15";
            const string sentence2 = "!AIVDM,2,2,8,,0Oh1IT0svT,0*7b";

            var message1 = Parser.Parse(sentence1);
            var message2 = Parser.Parse(sentence2);

            //    as UtcAndDateResponseMessage;
            //message.Should().NotBeNull();
            //message!.MessageType.Should().Be(AisMessageType.UtcAndDateResponse);
            //message.Repeat.Should().Be(0u);
            //message.Mmsi.Should().Be(240677000u);
            //message.Year.Should().Be(2019u);
            //message.Month.Should().Be(3u);
            //message.Day.Should().Be(22u);
            //message.Hour.Should().Be(6u);
            //message.Minute.Should().Be(51u);
            //message.Second.Should().Be(35u);
            //message.PositionAccuracy.Should().Be(PositionAccuracy.Low);
            //message.Longitude.Should().BeApproximately(0.07035d, 0.000001d);
            //message.Latitude.Should().BeApproximately(50.517017d, 0.000001d);
            //message.PositionFixType.Should().Be(PositionFixType.Gps);
            //message.Spare.Should().Be(0u);
            //message.Raim.Should().Be(Raim.NotInUse);
            //message.RadioStatus.Should().Be(0u);
        }
    }
}
