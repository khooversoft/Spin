using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions.test.Channel;

public class ChannelRecordTests
{
    [Fact]
    public void RoundTrip()
    {
        var dt = DateTime.UtcNow;

        var r1 = new ChannelRecord
        {
            ChannelId = "channel1",
            PrincipalGroupId = "channel1/principalGroup",
            Name = "Channel one",

            Messages = new List<ChannelMessage>
            {
                new ChannelMessage
                {
                    ChannelId = "channel1",
                    Date = dt,
                    Message = "Message one",
                    FromPrincipalId = "user2@domain.com",
                },
                new ChannelMessage
                {
                    ChannelId = "channel1",
                    Date = dt,
                    Message = "Message two",
                    FromPrincipalId = "user1@domain.com",
                },
            }
        };

        r1.Validate().IsOk().Should().BeTrue();

        var json = r1.ToJson();
        json.NotEmpty();

        var r2 = json.ToObject<ChannelRecord>();

        (r1 == r2).Should().BeTrue();
    }
}
