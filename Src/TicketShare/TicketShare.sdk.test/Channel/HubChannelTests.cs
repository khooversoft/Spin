//using FluentAssertions;
//using Microsoft.Extensions.DependencyInjection;
//using TicketShare.sdk.Applications;
//using Toolbox.Extensions;
//using Toolbox.Types;

//namespace TicketShare.sdk.test.Channel;

//public class HubChannelTests
//{
//    private const string _principalId = "sam@domain.com";
//    private const string _friend1 = "friend1@otherDomain.com";
//    private const string _friend2 = "friend2@otherDomain.com";
//    private const string _channelId = "channel1";

//    [Fact]
//    public async Task GeneralMessageFlow()
//    {
//        var testHost = new TicketShareTestHost();
//        var client = testHost.ServiceProvider.GetRequiredService<HubChannelClient>();
//        var context = testHost.GetScopeContext<HubChannelTests>();

//        var accountRecord = TestTool.CreateAccountModel(_principalId);
//        await TestTool.AddIdentityUser(accountRecord.PrincipalId, "samUser", testHost, context);
//        await TestTool.AddIdentityUser(_friend1, "friend-user1", testHost, context);
//        await TestTool.AddIdentityUser(_friend2, "friend-user2", testHost, context);
//        await AddHubChannel(client, _channelId, [_friend1, _friend2], context);

//        string messageId1 = await SendMessage(client, _friend1, "message #1", context);
//        string messageId2 = await SendMessage(client, _friend2, "message #2", context);
//        await TestChannelState(client, _principalId, 1, 2, context);
//        await TestChannelMessages(client, 2, context);

//        string messageId3 = await SendMessage(client, _principalId, "message #3", context);
//        await TestChannelState(client, _principalId, 1, 3, context);
//        await TestChannelMessages(client, 3, context);

//        await MarkRead(client, _channelId, _friend2, messageId1, context);
//        await TestChannelState(client, _principalId, 1, 3, context);
//        await TestChannelState(client, _friend2, 1, 2, context);
//        await TestChannelMessages(client, 3, context);
//    }

//    private async Task AddHubChannel(HubChannelClient client, string channelId, IEnumerable<string> users, ScopeContext context)
//    {
//        var hubChannel = new HubChannelRecord
//        {
//            ChannelId = channelId,
//            Users = new Dictionary<string, PrincipalChannelRecord>(StringComparer.OrdinalIgnoreCase)
//            {
//                [_principalId] = new PrincipalChannelRecord { PrincipalId = _principalId, Role = ChannelRole.Owner },
//                [_friend1] = new PrincipalChannelRecord { PrincipalId = _friend1, Role = ChannelRole.Contributor },
//                [_friend2] = new PrincipalChannelRecord { PrincipalId = _friend2, Role = ChannelRole.Contributor },
//            }
//        };

//        var result = await client.Add(hubChannel, context);
//        result.IsOk().Should().BeTrue(result.ToString());

//        await SearchAndVerify(client, _principalId, context);
//        await SearchAndVerify(client, _friend1, context);
//        await SearchAndVerify(client, _friend2, context);

//    }

//    private async Task<string> SendMessage(HubChannelClient client, string fromPrincipalId, string message, ScopeContext context)
//    {
//        var channelMessage = new ChannelMessageRecord
//        {
//            FromPrincipalId = fromPrincipalId,
//            ChannelId = _channelId,
//            Message = message,
//        };

//        var result = await client.Message.Send(channelMessage, context);
//        result.IsOk().Should().BeTrue(result.ToString());

//        var readOption = await client.Get(_channelId, context);
//        readOption.IsOk().Should().BeTrue();
//        var read = readOption.Return();

//        var messages = read.GetMessages();
//        messages.Any(x => x.Message.Message == message).Should().BeTrue();

//        return channelMessage.MessageId;
//    }

//    async Task SearchAndVerify(HubChannelClient client, string principalId, ScopeContext context)
//    {
//        var readOption = await client.GetByPrincipalId(principalId, context);
//        readOption.IsOk().Should().BeTrue();
//        readOption.Return().Action(read =>
//        {
//            read.Count.Should().Be(1);
//            read.Single().Action(x =>
//            {
//                x.ChannelId.Should().Be(_channelId);
//                x.Users.Count.Should().Be(3);
//                x.Users.Values.Count(x => x.Role == ChannelRole.Contributor).Should().Be(2);
//                x.Users.Values.Count(x => x.Role == ChannelRole.Owner).Should().Be(1);
//            });
//        });
//    }

//    private static async Task TestChannelState(HubChannelClient client, string principalId, int count, int unReadCount, ScopeContext context)
//    {
//        var channelStatesOption = await client.Message.GetChannelStatesForPrincipalId(principalId, context);
//        channelStatesOption.IsOk().Should().BeTrue();
//        var channelStates = channelStatesOption.Return();

//        channelStates.Count.Should().Be(count);
//        channelStates.Sum(x => x.UnReadMessages).Should().Be(unReadCount);
//    }

//    private async Task MarkRead(HubChannelClient client, string channelId, string principalId, string messageId1, ScopeContext context)
//    {
//        var option = await client.Message.MarkRead(channelId, principalId, [messageId1], DateTime.UtcNow, context);
//        option.IsOk().Should().BeTrue();
//    }

//    private async Task TestChannelMessages(HubChannelClient client, int messageCount, ScopeContext context)
//    {
//        var readOption = await client.Get(_channelId, context);
//        readOption.IsOk().Should().BeTrue();

//        var read = readOption.Return();
//        read.Messages.Count.Should().Be(messageCount);
//    }
//}
