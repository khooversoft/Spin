using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Should;

namespace TicketShare.sdk.test.Channel;

public class HubChannelModelTests
{
    [Fact]
    public void EmptyCompare()
    {
        var m1 = new HubChannelRecord();
        var m2 = new HubChannelRecord();

        (m1 == m2).Should().BeTrue();
    }

    [Fact]
    public void ChannelMessageRecordEquals()
    {
        DateTime dt = DateTime.UtcNow;

        var m1 = new ChannelMessageRecord
        {
            ChannelId = "hub-channel",
            MessageId = "message1",
            Date = dt,
            FromPrincipalId = "user1@domain.com",
            Message = "message1",
        };

        var m2 = new ChannelMessageRecord
        {
            MessageId = "message1",
            Date = dt,
            FromPrincipalId = "user1@domain.com",
            ChannelId = "hub-channel",
            Message = "message1",
        };

        (m1 == m2).Should().BeTrue();

        var m3 = new ChannelMessageRecord
        {
            MessageId = "message2",
            Date = dt,
            FromPrincipalId = "user1@domain.com",
            ChannelId = "hub-channel",
            Message = "message1",
        };

        (m1 == m3).Should().BeFalse();
    }

    [Fact]
    public void UserEquals()
    {
        const string principalId = "user1@company.com";
        const string user2PrincipalId = "user2@company.com";
        var date = DateTime.UtcNow;

        var m1 = new HubChannelRecord
        {
            ChannelId = "company.com/team1",
            Users = new Dictionary<string, PrincipalRoleRecord>(StringComparer.OrdinalIgnoreCase)
            {
                [principalId] = new PrincipalRoleRecord
                {
                    PrincipalId = principalId,
                    Role = ChannelRole.Owner,
                },
                [user2PrincipalId] = new PrincipalRoleRecord
                {
                    PrincipalId = user2PrincipalId,
                    Role = ChannelRole.Contributor,
                    LastMessageIdRead = "dd"
                }
            },
        };

        var m2 = new HubChannelRecord
        {
            ChannelId = "company.com/team1",
            Users = new Dictionary<string, PrincipalRoleRecord>(StringComparer.OrdinalIgnoreCase)
            {
                [principalId] = new PrincipalRoleRecord
                {
                    PrincipalId = principalId,
                    Role = ChannelRole.Owner,
                },
                [user2PrincipalId] = new PrincipalRoleRecord
                {
                    PrincipalId = user2PrincipalId,
                    Role = ChannelRole.Contributor,
                    LastMessageIdRead = "dd"
                }
            },
        };

        (m1 == m2).Should().BeTrue();
    }

    [Fact]
    public void FullModel()
    {
        const string principalId = "user1@company.com";
        const string user2PrincipalId = "user2@company.com";
        var date = DateTime.UtcNow;

        var model = new HubChannelRecord
        {
            ChannelId = "company.com/team1",
            Users = new Dictionary<string, PrincipalRoleRecord>(StringComparer.OrdinalIgnoreCase)
            {
                [principalId] = new PrincipalRoleRecord
                {
                    PrincipalId = principalId,
                    Role = ChannelRole.Owner,
                    LastMessageIdRead = "dd"
                },
                [user2PrincipalId] = new PrincipalRoleRecord
                {
                    PrincipalId = user2PrincipalId,
                    Role = ChannelRole.Contributor,
                    LastMessageIdRead = "dd"
                }
            },
            Messages = [
                new ChannelMessageRecord
                {
                    MessageId = "message1",
                    Date = date,
                    FromPrincipalId = user2PrincipalId,
                    ChannelId = "company.com/team1",
                    Message = "hello",
                },
                new ChannelMessageRecord
                {
                    MessageId = "message2",
                    Date = date,
                    FromPrincipalId = user2PrincipalId,
                    ChannelId = "company.com/team1",
                    Message = "hello2",
                },
                new ChannelMessageRecord
                {
                    MessageId = "message3",
                    Date = date,
                    FromPrincipalId = "user1@company.com",
                    ChannelId = "company.com/team1",
                    Message = "hello from owner",
                }
            ],
        };

        var json = model.ToJson();

        var readModel = json.ToObject<HubChannelRecord>().NotNull();
        (model == readModel).Should().BeTrue();
    }
}
