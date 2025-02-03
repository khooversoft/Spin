using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Graph.Extensions.test.Tools;
using Toolbox.Graph.Extensions.Testing;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions.test.Channel;

public class ChannelClientTests
{
    private const string _user1 = "user1@domain.com";
    private const string _user2 = "user2@domain.com";
    private const string _user3 = "user3@domain.com";
    private const string _user4 = "user4@domain.com";
    private const string _groupid1 = "groupid1";
    private const string _groupid2 = "groupid2";
    private const string _channel1 = "channel1";
    private const string _channel2 = "channel2";
    private const string _principalGroup1 = "securityGroup1";
    private const string _principalGroup2 = "securityGroup2";

    [Fact]
    public async Task Lifecycle()
    {
        var testHost = new ToolboxExtensionTestHost();
        var groupClient = testHost.ServiceProvider.GetRequiredService<SecurityGroupClient>();
        var channelClient = testHost.ServiceProvider.GetRequiredService<ChannelClient>();
        IGraphClient graphClient = testHost.ServiceProvider.GetRequiredService<IGraphClient>();
        var context = testHost.GetScopeContext<ChannelClientTests>();

        await IdentityTestTool.AddIdentityUser(_user1, "user 1", testHost, context);

        // Create security group with user for access
        (await groupClient.Create(_groupid1, "group 1", [(_user1, SecurityAccess.Owner)], context)).IsOk().Should().BeTrue();

        // Because security group has already created, this should just attach the channel to it.
        var channel = new ChannelRecord
        {
            ChannelId = _channel1,
            SecurityGroupId = _groupid1,
            Name = "Channel one",
        };

        (await channelClient.Create(channel.ChannelId, channel.SecurityGroupId, channel.Name, context)).IsOk().Should().BeTrue();

        var readOption = await channelClient.GetContext(_channel1, _user1).Get(context);
        readOption.IsOk().Should().BeTrue(readOption.ToString());
        (channel == readOption.Return()).Should().BeTrue();

        var newchannelRecord = channel with
        {
            Name = "new name",
            Messages = [
                new ChannelMessage { ChannelId = _channel1, MessageId = "message1", FromPrincipalId = _user1, Message = "message 1" }
                ]
        };

        var setResult = await channelClient.GetContext(_channel1, _user1).Set(newchannelRecord, context);
        setResult.IsOk().Should().BeTrue();

        readOption = await channelClient.GetContext(_channel1, _user1).Get(context);
        readOption.IsOk().Should().BeTrue();
        (newchannelRecord == readOption.Return()).Should().BeTrue();

        // Because user has access to the secuity group, the user access access
        var listOption = await channelClient.GetPrincipalChannels(_user1, context);
        listOption.IsOk().Should().BeTrue();
        listOption.Return().Action(x =>
        {
            x.Count.Should().Be(1);
            x[0].ChannelId.Should().Be(_channel1);
        });

        var deleteOption = await channelClient.GetContext(_channel1, _user1).Delete(context);
        deleteOption.IsOk().Should().BeTrue();

        readOption = await channelClient.GetContext(_channel1, _user1).Get(context);
        readOption.IsNotFound().Should().BeTrue();
    }

    [Fact]
    public async Task SecurityGroupSearch()
    {
        var testHost = new ToolboxExtensionTestHost();
        var securityClient = testHost.ServiceProvider.GetRequiredService<SecurityGroupClient>();
        var channelClient = testHost.ServiceProvider.GetRequiredService<ChannelClient>();
        IGraphClient graphClient = testHost.ServiceProvider.GetRequiredService<IGraphClient>();
        var context = testHost.GetScopeContext<ChannelClientTests>();

        await IdentityTestTool.AddIdentityUser(_user1, "user 1", testHost, context);
        await IdentityTestTool.AddIdentityUser(_user2, "user 2", testHost, context);
        await IdentityTestTool.AddIdentityUser(_user3, "user 3", testHost, context);
        await IdentityTestTool.AddIdentityUser(_user4, "user 4", testHost, context);

        (await securityClient.Create(_principalGroup1, "group 1", [(_user1, SecurityAccess.Reader), (_user2, SecurityAccess.Owner)], context)).IsOk().Should().BeTrue();
        (await securityClient.Create(_principalGroup2, "group 2", [(_user1, SecurityAccess.Reader), (_user3, SecurityAccess.Owner)], context)).IsOk().Should().BeTrue();
        (await channelClient.Create(_channel1, _principalGroup1, "channel 1", context)).IsOk().Should().BeTrue();
        (await channelClient.Create(_channel2, _principalGroup2, "channel 2", context)).IsOk().Should().BeTrue();

        (await channelClient.GetPrincipalChannels(_user1, context)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Action(y =>
            {
                y.Count.Should().Be(2);
                y.Select(x => x.ChannelId).OrderBy(x => x).SequenceEqual([_channel1, _channel2]).Should().BeTrue();
            });
        });

        (await channelClient.GetPrincipalChannels(_user2, context)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Action(y =>
            {
                y.Count.Should().Be(1);
                y.Select(x => x.ChannelId).OrderBy(x => x).SequenceEqual([_channel1]).Should().BeTrue();
            });
        });

        (await channelClient.GetPrincipalChannels(_user3, context)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Action(y =>
            {
                y.Count.Should().Be(1);
                y.Select(x => x.ChannelId).OrderBy(x => x).SequenceEqual([_channel2]).Should().BeTrue();
            });
        });

        (await channelClient.GetPrincipalChannels(_user4, context)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Count.Should().Be(0);
        });
    }

    private async Task CreateSecurityGroup(
        SecurityGroupClient client,
        string groupId,
        string name,
        IEnumerable<(string principalId, SecurityAccess access)> members,
        ScopeContext context
        )
    {
        var groupRecord = new SecurityGroupRecord
        {
            SecurityGroupId = groupId,
            Name = name,
            Members = members.NotNull()
                .Select(x => new PrincipalAccess { PrincipalId = x.principalId, Access = x.access })
                .ToDictionary(x => x.PrincipalId),
        };

        var addResult = await client.Create(groupRecord, context);
        addResult.IsOk().Should().BeTrue();
    }
}
