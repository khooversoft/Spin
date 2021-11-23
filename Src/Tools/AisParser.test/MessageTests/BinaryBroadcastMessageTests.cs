using AisParser;
using AisParser.Messages;
using FluentAssertions;
using Xunit;

namespace AisParserTests.MessagesTests
{
    public class BinaryBroadcastMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence1 = "!AIVDM,2,1,6,A,83Ksgb12@@0bJvW?NL8I4dOuvga6>QTBjkQg>:sK6A;>?bGuDkDI7q:626ud,0*6D";
            const string sentence2 = "!AIVDM,2,2,6,A,g@0,2*05";

            Parser.Parse(sentence1).Should().BeNull();
            var message = Parser.Parse(sentence2) as BinaryBroadcastMessage;
            message.Should().NotBeNull();
            message!.MessageType.Should().Be(AisMessageType.BinaryBroadcastMessage);
            message.Repeat.Should().Be(0u);
            message.Mmsi.Should().Be(230617000u);
            message.DesignatedAreaCode.Should().Be(265u);
            message.FunctionalId.Should().Be(1u);
            message.Data.Should().Be("B)+:\\=90!$R1?7:>$X:FQKKNF<8+-,YD,8>)_5SMQ$_$(XH[62=");
        }
    }
}