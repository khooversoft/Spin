using Microsoft.Extensions.DependencyInjection;
using TicketShare.sdk.Applications;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace TicketShare.sdk.test.Channel;

public class HubChannelTests
{
    private const string _principalId = "sam@domain.com";
    private const string _friend1 = "friend1@otherDomain.com";
    private const string _friend2 = "friend2@otherDomain.com";
    private const string _channelId = "channel1";

    [Fact]
    public async Task GeneralMessageFlow()
    {
        var testHost = new TicketShareTestHost();
        var manager = testHost.ServiceProvider.GetRequiredService<HubChannelManager>();
        var context = testHost.GetScopeContext<HubChannelTests>();

        var accountRecord = TestTool.CreateAccountModel(_principalId);
        await TestTool.AddIdentityUser(accountRecord.PrincipalId, "samUser", testHost, context);
        await TestTool.AddIdentityUser(_friend1, "friend-user1", testHost, context);
        await TestTool.AddIdentityUser(_friend2, "friend-user2", testHost, context);
        await AddHubChannel(manager, _channelId, [_friend1, _friend2], context);

        string messageId1 = await SendMessage(manager, _friend1, "message #1", context);
        string messageId2 = await SendMessage(manager, _friend2, "message #2", context);
        await TestChannelMessages(manager, 2, context);

        string messageId3 = await SendMessage(manager, _principalId, "message #3", context);
        await TestChannelMessages(manager, 3, context);

        await SearchByUser(manager, _principalId, context);
        await SearchByUser(manager, _friend1, context);
        await SearchByUser(manager, _friend2, context);
    }

    private async Task SearchByUser(HubChannelManager manager, string principalId, ScopeContext context)
    {
        var searchResult = await manager.GetByPrincipalId(principalId, context);
        searchResult.IsOk().Should().BeTrue();

        var search = searchResult.Return();
        search.Count.Should().Be(1);

        var infos = await manager.GetChannelsInfo(principalId, context);
        infos.IsOk().Should().BeTrue();
        infos.Return().Count.Should().Be(1);
    }

    private async Task AddHubChannel(HubChannelManager manager, string channelId, IEnumerable<string> users, ScopeContext context)
    {
        var createOption = await manager.CreateChannel(channelId, "name1", _principalId, context);
        createOption.IsOk().Should().BeTrue();

        var addFriend1Option = await manager.Principal.Set(_principalId, channelId, _friend1, ChannelRole.Contributor, context);
        addFriend1Option.IsOk().Should().BeTrue();

        var addFriend2Option = await manager.Principal.Set(_principalId, channelId, _friend2, ChannelRole.Contributor, context);
        addFriend2Option.IsOk().Should().BeTrue();

        var hubChannel = new HubChannelRecord
        {
            ChannelId = channelId,
            Name = "name1",
            Users = new Dictionary<string, PrincipalChannelRecord>(StringComparer.OrdinalIgnoreCase)
            {
                [_principalId] = new PrincipalChannelRecord { PrincipalId = _principalId, Role = ChannelRole.Owner },
                [_friend1] = new PrincipalChannelRecord { PrincipalId = _friend1, Role = ChannelRole.Contributor },
                [_friend2] = new PrincipalChannelRecord { PrincipalId = _friend2, Role = ChannelRole.Contributor },
            }
        };

        var result = await manager.Get(_principalId, channelId, context);
        result.IsOk().Should().BeTrue(result.ToString());

        await SearchAndVerify(manager, _principalId, context);
        await SearchAndVerify(manager, _friend1, context);
        await SearchAndVerify(manager, _friend2, context);

    }

    private async Task<string> SendMessage(HubChannelManager manager, string fromPrincipalId, string message, ScopeContext context)
    {
        var channelMessage = new ChannelMessageRecord
        {
            FromPrincipalId = fromPrincipalId,
            ChannelId = _channelId,
            Message = message,
        };

        var result = await manager.Messages.Send(channelMessage, context);
        result.IsOk().Should().BeTrue(result.ToString());

        var readOption = await manager.Get(_principalId, _channelId, context);
        readOption.IsOk().Should().BeTrue();
        var read = readOption.Return();
        read.Messages.Count.Assert(x => x > 0, _ => "Empty messages set");
        read.Messages.Any(read => read.Message == message).Should().BeTrue();

        var messages = await manager.Messages.Get(fromPrincipalId, _channelId, context);
        messages.IsOk().Should().BeTrue();

        messages.Return().Any(x => x.Message == message).Should().BeTrue();

        return channelMessage.MessageId;
    }

    async Task SearchAndVerify(HubChannelManager manager, string principalId, ScopeContext context)
    {
        var readOption = await manager.GetByPrincipalId(principalId, context);
        readOption.IsOk().Should().BeTrue();

        readOption.Return().Action(read =>
        {
            read.Count.Should().Be(1);
            read.Single().Action(x =>
            {
                x.ChannelId.Should().Be(_channelId);
                x.Users.Count.Should().Be(3);
                x.Users.Values.Count(x => x.Role == ChannelRole.Contributor).Should().Be(2);
                x.Users.Values.Count(x => x.Role == ChannelRole.Owner).Should().Be(1);
            });
        });
    }

    private async Task TestChannelMessages(HubChannelManager manager, int messageCount, ScopeContext context)
    {
        var readOption = await manager.Messages.Get(_principalId, _channelId, context);
        readOption.IsOk().Should().BeTrue();

        var read = readOption.Return();
        read.Count.Should().Be(messageCount);
    }
}
