//using Microsoft.Extensions.DependencyInjection;
//using Toolbox.Extensions;
//using Toolbox.Graph.Extensions.test.Application;
//using Toolbox.Graph.Extensions.test.Tools;
//using Toolbox.Tools;
//using Toolbox.Types;
//using Xunit.Abstractions;

//namespace Toolbox.Graph.Extensions.test.Channel;

//public class ChannelMessageTests
//{
//    private const string _user1 = "user1@domain.com";
//    private const string _user2 = "user2@domain.com";
//    private const string _user3 = "user3@domain.com";
//    private const string _user4 = "user4@domain.com";
//    private const string _groupid1 = "groupid1";
//    private const string _groupid2 = "groupid2";
//    private const string _channel1 = "channel1";
//    private const string _channel2 = "channel2";
//    private const string _principalGroup1 = "securityGroup1";
//    private const string _principalGroup2 = "securityGroup2";
//    private readonly ITestOutputHelper _outputHelper;

//    public ChannelMessageTests(ITestOutputHelper outputHelper)
//    {
//        _outputHelper = outputHelper;
//    }

//    [Fact]
//    public async Task WriteAndReadMessages()
//    {
//        var graphHostService = await TestHost.Create(_outputHelper);
//        var context = graphHostService.CreateScopeContext<ChannelMessageTests>();

//        var groupClient = graphHostService.Services.GetRequiredService<SecurityGroupClient>();
//        var channelClient = graphHostService.Services.GetRequiredService<ChannelClient>();

//        await IdentityTestTool.AddIdentityUser(_user1, "user 1", graphHostService, context);

//        // Create security group with user for access
//        (await groupClient.Create(_groupid1, "group 1", [(_user1, SecurityAccess.Owner)], context)).IsOk().BeTrue();

//        // Because security group has already created, this should just attach the channel to it.
//        var channel = new ChannelRecord
//        {
//            ChannelId = _channel1,
//            SecurityGroupId = _groupid1,
//            Name = "Channel one",
//        };

//        var addResult = await channelClient.Create(channel, context);
//        addResult.IsOk().BeTrue(addResult.ToString());

//        var m1 = new ChannelMessage { ChannelId = _channel1, FromPrincipalId = _user1, Message = "message 1" };
//        (await channelClient.GetContext(_channel1, _user1).AddMessage(m1, context)).IsOk().BeTrue();

//        (await channelClient.GetContext(_channel1, _user1).GetMessages(context)).Action(x =>
//        {
//            x.IsOk().BeTrue();
//            x.Return().Action(y =>
//            {
//                y.Count.Be(1);
//                (y[0] == m1).BeTrue();
//            });
//        });

//        const int count = 3;
//        var newMessages = Enumerable.Range(0, count)
//            .Select(x => new ChannelMessage { ChannelId = _channel1, FromPrincipalId = _user1, Message = $"message {x}" })
//            .ToArray();

//        (await channelClient.GetContext(_channel1, _user1).AddMessages(newMessages, context)).IsOk().BeTrue();

//        (await channelClient.GetContext(_channel1, _user1).GetMessages(context)).Action(x =>
//        {
//            x.IsOk().BeTrue();
//            x.Return().Action(y =>
//            {
//                y.Count.Be(count + 1);
//                (y[0] == m1).BeTrue();
//                (y[1] == newMessages[0]).BeTrue();
//                (y[2] == newMessages[1]).BeTrue();
//                (y[3] == newMessages[2]).BeTrue();
//            });
//        });
//    }

//    [Fact]
//    public async Task WriteAndReadMultipleMessages()
//    {
//        var graphHostService = await TestHost.Create(_outputHelper);
//        var context = graphHostService.CreateScopeContext<ChannelMessageTests>();

//        var groupClient = graphHostService.Services.GetRequiredService<SecurityGroupClient>();
//        var channelClient = graphHostService.Services.GetRequiredService<ChannelClient>();

//        await IdentityTestTool.AddIdentityUser(_user1, "user 1", graphHostService, context);

//        // Create security group with user for access
//        await CreateSecurityGroup(groupClient, _groupid1, "group 1", [(_user1, SecurityAccess.Owner)], context);

//        // Because security group has already created, this should just attach the channel to it.
//        var channel = new ChannelRecord
//        {
//            ChannelId = _channel1,
//            SecurityGroupId = _groupid1,
//            Name = "Channel one",
//        };

//        var addResult = await channelClient.Create(channel, context);
//        addResult.IsOk().BeTrue(addResult.ToString());

//        var channelContext = channelClient.GetContext(_channel1, _user1);
//        const int count = 100;
//        int batchSize = 1;
//        var list = new Sequence<ChannelMessage>();
//        var track = new Sequence<int>();

//        while (list.Count < count)
//        {
//            var newMessages = Enumerable.Range(0, batchSize)
//                .Select(x => new ChannelMessage { ChannelId = _channel1, FromPrincipalId = _user1, Message = $"message {x}" })
//                .ToArray();

//            list += newMessages;

//            (await channelContext.AddMessages(newMessages, context)).IsOk().BeTrue();

//            (await channelContext.GetMessages(context)).Action(x =>
//            {
//                x.IsOk().BeTrue();
//                x.Return().Action(y =>
//                {
//                    y.Count.Be(list.Count);
//                    y.SequenceEqual(list).BeTrue();
//                });
//            });

//            batchSize += (int)(batchSize * 1.5);
//            track += batchSize;

//            batchSize = Math.Min(batchSize, count - list.Count);
//        }

//        (await channelClient.GetContext(_channel1, _user1).GetMessages(context)).Action(x =>
//        {
//            x.IsOk().BeTrue(x.ToString());
//            x.Return().Action(y =>
//            {
//                y.Count.Be(list.Count);
//                x.Return().Action(y =>
//                {
//                    y.Count.Be(list.Count);
//                    y.SequenceEqual(list).BeTrue();
//                });
//            });
//        });
//    }

//    private async Task CreateSecurityGroup(
//            SecurityGroupClient client,
//            string groupId,
//            string name,
//            IEnumerable<(string principalId, SecurityAccess access)> members,
//            ScopeContext context
//        )
//    {
//        var groupRecord = new SecurityGroupRecord
//        {
//            SecurityGroupId = groupId,
//            Name = name,
//            Members = members.NotNull()
//                .Select(x => new PrincipalAccess { PrincipalId = x.principalId, Access = x.access })
//                .ToDictionary(x => x.PrincipalId),
//        };

//        var addResult = await client.Create(groupRecord, context);
//        addResult.IsOk().BeTrue();
//    }
//}
