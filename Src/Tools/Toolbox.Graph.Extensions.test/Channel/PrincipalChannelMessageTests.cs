//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Toolbox.Extensions;
//using Toolbox.Graph.Extensions.test.Application;
//using Toolbox.Graph.Extensions.test.Tools;
//using Toolbox.Tools;
//using Toolbox.Types;
//using Xunit.Abstractions;

//namespace Toolbox.Graph.Extensions.test.Channel;

//public class PrincipalChannelMessageTests
//{
//    private const string _user4 = "user4@domain.com";
//    private const string _user1 = "user1@domain.com";
//    private const string _user2 = "user2@domain.com";
//    private const string _user3 = "user3@domain.com";
//    private const string _groupid1 = "groupid1";
//    private const string _groupid2 = "groupid2";
//    private const string _groupid3 = "groupid3";
//    private const string _channel1 = "channel1";
//    private const string _channel2 = "channel2";
//    private const string _channel3 = "channel3";
//    private const string _principalGroup1 = "securityGroup1";
//    private const string _principalGroup2 = "securityGroup2";
//    private readonly ITestOutputHelper _outputHelper;

//    private ChannelMessage[] _channelMessages = Enumerable.Range(0, 100)
//        .Select(x => new ChannelMessage { ChannelId = _channel1, FromPrincipalId = _user1, Message = $"Message #{x}" })
//        .ToArray();

//    public PrincipalChannelMessageTests(ITestOutputHelper outputHelper)
//    {
//        _outputHelper = outputHelper;
//    }

//    [Fact]
//    public async Task SingleChannel()
//    {
//        var graphHostService = await TestHost.Create(_outputHelper);
//        var context = graphHostService.CreateScopeContext<PrincipalChannelMessageTests>();

//        var groupClient = graphHostService.Services.GetRequiredService<SecurityGroupClient>();
//        var channelClient = graphHostService.Services.GetRequiredService<ChannelClient>();

//        await IdentityTestTool.AddIdentityUser(_user1, "user 1", graphHostService, context);
//        await CreateSecurityGroup(groupClient, _groupid1, "group 1", [(_user1, SecurityAccess.Owner)], context);

//        // Create security group with user for access
//        await CreateChannel(channelClient, _channel1, _groupid1, "Channel one", context);

//        int messageIndex = 0;
//        using (var metric = context.LogDuration("Add-messages", "message count={count}", _channelMessages.Length))
//        {
//            (messageIndex, _) = await AddMessage(channelClient, _channel1, _user1, messageIndex, _channelMessages.Length, context);
//        }

//        using (var metric = context.LogDuration("Get-messages-ms"))
//        {
//            (await channelClient.GetPrincipalMessages(_user1, context)).Action(x =>
//            {
//                x.IsOk().BeTrue();
//                x.Return().Action(y =>
//                {
//                    context.LogInformation("Count={count}", y.Count);
//                    y.Count.Be(messageIndex);
//                    _channelMessages.Take(messageIndex).IsEquivalent(y).BeTrue();
//                });
//            });
//        }
//    }

//    [Fact]
//    public async Task SingleChannelMultipleSizeWrites()
//    {
//        var graphHostService = await TestHost.Create(_outputHelper);
//        var context = graphHostService.CreateScopeContext<PrincipalChannelMessageTests>();

//        var groupClient = graphHostService.Services.GetRequiredService<SecurityGroupClient>();
//        var channelClient = graphHostService.Services.GetRequiredService<ChannelClient>();

//        await IdentityTestTool.AddIdentityUser(_user1, "user 1", graphHostService, context);
//        await CreateSecurityGroup(groupClient, _groupid1, "group 1", [(_user1, SecurityAccess.Owner)], context);

//        // Create security group with user for access
//        await CreateChannel(channelClient, _channel1, _groupid1, "Channel one", context);

//        int messageIndex = 0;
//        int size = 1;
//        var batches = new Sequence<ChannelMessage[]>();

//        while (messageIndex < _channelMessages.Length)
//        {
//            var messagesToSend = _channelMessages.Skip(messageIndex).Take(size).ToArray();
//            messageIndex += size;
//            batches += messagesToSend;

//            size = (int)(size * 1.1) + 1;
//            size = Math.Min(size, _channelMessages.Length - messageIndex);
//        }

//        int shouldCount = 0;
//        foreach (var batch in batches)
//        {
//            shouldCount += batch.Length;
//            using (var metric = context.LogDuration("Add-messages", "message count={count}", batch.Length))
//            {
//                (await channelClient.GetContext(_channel1, _user1).AddMessages(batch, context)).IsOk().BeTrue();
//            }

//            using (var metric = context.LogDuration("Get-messages-ms"))
//            {
//                (await channelClient.GetPrincipalMessages(_user1, context)).Action(x =>
//                {
//                    x.IsOk().BeTrue();
//                    x.Return().Action(y =>
//                    {
//                        context.LogInformation("Count={count}", y.Count);
//                        y.Count.Be(shouldCount);
//                        _channelMessages.Take(shouldCount).IsEquivalent(y).BeTrue();
//                    });
//                });
//            }
//        }
//    }

//    [Fact]
//    public async Task MultipleChannels()
//    {
//        var graphHostService = await TestHost.Create(_outputHelper);
//        var context = graphHostService.CreateScopeContext<PrincipalChannelMessageTests>();

//        var groupClient = graphHostService.Services.GetRequiredService<SecurityGroupClient>();
//        var channelClient = graphHostService.Services.GetRequiredService<ChannelClient>();

//        await IdentityTestTool.AddIdentityUser(_user1, "user 1", graphHostService, context);
//        await IdentityTestTool.AddIdentityUser(_user2, "user 2", graphHostService, context);
//        await IdentityTestTool.AddIdentityUser(_user3, "user 3", graphHostService, context);
//        await CreateSecurityGroup(groupClient, _groupid1, "group 1", [(_user1, SecurityAccess.Owner)], context);
//        await CreateSecurityGroup(groupClient, _groupid2, "group 2", [(_user2, SecurityAccess.Owner)], context);
//        await CreateSecurityGroup(groupClient, _groupid3, "Allusers", [(_user3, SecurityAccess.Owner)], context);

//        // Create security group with user for access
//        await CreateChannel(channelClient, _channel1, _groupid1, "User's 1 channel", context);
//        await CreateChannel(channelClient, _channel2, _groupid2, "User's 2 channel", context);
//        await CreateChannel(channelClient, _channel3, _groupid3, "User's 3 channel", context);

//        int messageIndex = 0;
//        int size = 1;
//        int userIndex = 0;
//        (string channel, string user)[] users = [(_channel1, _user1), (_channel2, _user2), (_channel3, _user3)];
//        var usersMessages = new Sequence<(string user, ChannelMessage[] messages)>();

//        while (messageIndex < _channelMessages.Length)
//        {
//            (string channel, string user) = users[userIndex++ % users.Length];

//            using (var metric = context.LogDuration("Add-messages", "message count={count}", size))
//            {
//                (messageIndex, ChannelMessage[] msgs) = await AddMessage(channelClient, channel, user, messageIndex, size, context);
//                usersMessages += (user, msgs);
//            }

//            size = (int)(size * 1.1) + 1;
//        }

//        var group = usersMessages.GroupBy(x => x.user);

//        foreach (var item in group)
//        {
//            using (var metric = context.LogDuration("Get-messages", "user={user}", item.Key))
//            {
//                (await channelClient.GetPrincipalMessages(item.Key, context)).Action(x =>
//                {
//                    x.IsOk().BeTrue();
//                    x.Return().Action(y =>
//                    {
//                        context.LogInformation("Count={count}", y.Count);
//                        var messages = item.SelectMany(x => x.messages).ToArray();
//                        y.Count.Be(messages.Length);
//                        messages.IsEquivalent(y).BeTrue();
//                    });
//                });
//            }
//        }
//    }

//    private async Task CreateSecurityGroup(
//        SecurityGroupClient client,
//        string groupId,
//        string name,
//        IEnumerable<(string principalId, SecurityAccess access)> members,
//        ScopeContext context
//    )
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

//    private async Task CreateChannel(ChannelClient client, string channelId, string securityGroupId, string name, ScopeContext context)
//    {
//        var channelRecord = new ChannelRecord
//        {
//            ChannelId = channelId,
//            SecurityGroupId = securityGroupId,
//            Name = name,
//        };
//        var addResult = await client.Create(channelRecord, context);
//        addResult.IsOk().BeTrue();
//    }

//    private async Task<(int index, ChannelMessage[] messages)> AddMessage(ChannelClient client, string channelId, string userId, int index, int size, ScopeContext context)
//    {
//        size = Math.Min(size, _channelMessages.Length - index);
//        int end = index + size;
//        using var metric = context.LogDuration("add-message", "index={index}, size={size}", index, size);

//        var messagesToSend = _channelMessages.Skip(index).Take(size).ToArray();
//        (await client.GetContext(channelId, userId).AddMessages(messagesToSend, context)).IsOk().BeTrue();

//        return (index + size, messagesToSend);
//    }
//}
