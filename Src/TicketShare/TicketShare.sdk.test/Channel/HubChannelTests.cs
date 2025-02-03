//using Microsoft.Extensions.DependencyInjection;
//using TicketShare.sdk.Applications;
//using Toolbox.Extensions;
//using Toolbox.Graph.Extensions;
//using Toolbox.Tools;
//using Toolbox.Tools.Should;
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
//        var channelClient = testHost.ServiceProvider.GetRequiredService<ChannelClient>();
//        var context = testHost.GetScopeContext<HubChannelTests>();

//        var accountRecord = TestTool.CreateAccountModel(_principalId);
//        await TestTool.AddIdentityUser(accountRecord.PrincipalId, "samUser", testHost, context);
//        await TestTool.AddIdentityUser(_friend1, "friend-user1", testHost, context);
//        await TestTool.AddIdentityUser(_friend2, "friend-user2", testHost, context);
//        await AddHubChannel(channelClient, _channelId, [_friend1, _friend2], context);

//        string messageId1 = await SendMessage(manager, channelClient, _friend1, "message #1", context);
//        string messageId2 = await SendMessage(manager, channelClient, _friend2, "message #2", context);
//        await TestChannelMessages(manager, channelClient, 2, context);

//        string messageId3 = await SendMessage(manager, channelClient, _principalId, "message #3", context);
//        await TestChannelMessages(manager, channelClient, 3, context);

//        await SearchByUser(manager, channelClient, _principalId, context);
//        await SearchByUser(manager, channelClient, _friend1, context);
//        await SearchByUser(manager, channelClient, _friend2, context);
//    }

//    private async Task SearchByUser(HubChannelManager manager, HubChannelClient client, string principalId, ScopeContext context)
//    {
//        var searchResult = await client.GetByPrincipalId(principalId, context);
//        searchResult.IsOk().Should().BeTrue();

//        var search = searchResult.Return();
//        search.Count.Should().Be(1);

//        var infos = await manager.GetChannelsInfo(principalId, context);
//        infos.IsOk().Should().BeTrue();
//        infos.Return().Count.Should().Be(1);
//    }

//    private async Task AddHubChannel(ChannelClient channelClient, string channelId, IEnumerable<string> users, ScopeContext context)
//    {
//        var createOption = await channelClient.Create(channelId, "name1", _principalId, context);
//        createOption.IsOk().Should().BeTrue();

//        var channelContext = channelClient.GetContext(_channelId, _principalId);

//        var addFriend1Option = await channelContext.Principals.Set(_friend1, ChannelRole.Contributor, context);
//        addFriend1Option.IsOk().Should().BeTrue();

//        var addFriend2Option = await channelContext.Principals.Set(_friend2, ChannelRole.Contributor, context);
//        addFriend2Option.IsOk().Should().BeTrue();

//        var hubChannel = new HubChannelRecord
//        {
//            ChannelId = channelId,
//            Name = "name1",
//            Users = new Dictionary<string, PrincipalRoleRecord>(StringComparer.OrdinalIgnoreCase)
//            {
//                [_principalId] = new PrincipalRoleRecord { PrincipalId = _principalId, Role = ChannelRole.Owner },
//                [_friend1] = new PrincipalRoleRecord { PrincipalId = _friend1, Role = ChannelRole.Contributor },
//                [_friend2] = new PrincipalRoleRecord { PrincipalId = _friend2, Role = ChannelRole.Contributor },
//            }
//        };

//        var result = await channelContext.Get(context);
//        result.IsOk().Should().BeTrue(result.ToString());

//        await SearchAndVerify(manager, client, _principalId, context);
//        await SearchAndVerify(manager, client, _friend1, context);
//        await SearchAndVerify(manager, client, _friend2, context);

//    }

//    private async Task<string> SendMessage(HubChannelManager manager, HubChannelClient client, string fromPrincipalId, string message, ScopeContext context)
//    {
//        var hubContext = manager.GetContext(_channelId, _principalId, context);

//        var sendResult = await hubContext.Messages.Send(message, context);
//        sendResult.IsOk().Should().BeTrue(sendResult.ToString());

//        var readOption = await client.Get(_channelId, context);
//        readOption.IsOk().Should().BeTrue();
//        var read = readOption.Return();
//        read.Messages.Count.Assert(x => x > 0, _ => "Empty messages set");
//        read.Messages.Any(read => read.Message == message).Should().BeTrue();

//        var messages = await hubContext.Messages.Get(context);
//        messages.IsOk().Should().BeTrue();
//        messages.Return().Any(x => x.Message == message).Should().BeTrue();

//        return sendResult.Return();
//    }

//    async Task SearchAndVerify(HubChannelManager manager, HubChannelClient client, string principalId, ScopeContext context)
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

//    private async Task TestChannelMessages(HubChannelManager manager, HubChannelClient client, int messageCount, ScopeContext context)
//    {
//        var readOption = await client.Get(_channelId, context);
//        readOption.IsOk().Should().BeTrue();

//        var read = readOption.Return();
//        read.Messages.Count.Should().Be(messageCount);

//        var hubContext = manager.GetContext(_channelId, _principalId, context);
//        var messages = await hubContext.Messages.Get(context);
//        messages.IsOk().Should().BeTrue();

//        var messageList = messages.Return();
//        messageList.Count.Should().Be(messageCount);
//    }
//}
