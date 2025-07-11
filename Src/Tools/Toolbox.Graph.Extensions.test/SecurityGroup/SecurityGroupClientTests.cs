using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Graph.Extensions.test.Application;
using Toolbox.Graph.Extensions.test.Tools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.Extensions.test.PrincipalGroup;

public class SecurityGroupClientTests
{
    private const string _user1 = "user1@domain.com";
    private const string _user2 = "user2@domain.com";
    private const string _user3 = "user3@domain.com";
    private const string _user4 = "user4@domain.com";
    private const string _groupid1 = "groupid1";
    private const string _groupid2 = "groupid2";
    private readonly ITestOutputHelper _outputHelper;

    public SecurityGroupClientTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public async Task Lifecycle()
    {
        var graphHostService = await TestHost.Create(_outputHelper);
        var context = graphHostService.CreateScopeContext<SecurityGroupClientTests>();

        var groupClient = graphHostService.Services.GetRequiredService<SecurityGroupClient>();
        var channelClient = graphHostService.Services.GetRequiredService<ChannelClient>();

        await IdentityTestTool.AddIdentityUser(_user1, "user 1", graphHostService, context);
        await IdentityTestTool.AddIdentityUser(_user2, "user 2", graphHostService, context);

        var groupRecord = CreateGroup(_groupid1, "group 1", [(_user1, SecurityAccess.Reader), (_user2, SecurityAccess.Owner)]);

        var addResult = await groupClient.Create(groupRecord, context);
        addResult.IsOk().BeTrue();

        var readOption = await groupClient.GetContext(_groupid1, _user1).Get(context);
        readOption.IsOk().BeTrue();
        (groupRecord == readOption.Return()).BeTrue();

        var newGroupRecord = groupRecord with
        {
            Name = "new name"
        };

        var updateOption = await groupClient.GetContext(_groupid1, _user2).Set(newGroupRecord, context);
        updateOption.IsOk().BeTrue();

        readOption = await groupClient.GetContext(_groupid1, _user2).Get(context);
        readOption.IsOk().BeTrue();
        (newGroupRecord == readOption.Return()).BeTrue();

        var listOption = await groupClient.GroupsForPrincipalId(_user1, context);
        listOption.IsOk().BeTrue();
        listOption.Return().Action(x =>
        {
            x.Count.Be(1);
            x[0].Be(_groupid1);
        });

        var deleteOption = await groupClient.GetContext(_groupid1, _user2).Delete(context);
        deleteOption.IsOk().BeTrue();

        readOption = await groupClient.GetContext(_groupid1, _user2).Get(context);
        readOption.IsNotFound().BeTrue();
    }

    [Fact]
    public async Task LifecycleWithContext()
    {
        var graphHostService = await TestHost.Create(_outputHelper);
        var context = graphHostService.CreateScopeContext<SecurityGroupClientTests>();

        var groupClient = graphHostService.Services.GetRequiredService<SecurityGroupClient>();
        var channelClient = graphHostService.Services.GetRequiredService<ChannelClient>();

        await IdentityTestTool.AddIdentityUser(_user1, "user 1", graphHostService, context);
        await IdentityTestTool.AddIdentityUser(_user2, "user 2", graphHostService, context);

        var groupRecord = CreateGroup(_groupid1, "group 1", [(_user1, SecurityAccess.Reader), (_user2, SecurityAccess.Owner)]);

        var addResult = await groupClient.Create(groupRecord, context);
        addResult.IsOk().BeTrue();

        var readOption = await groupClient.GetContext(_groupid1, _user1).Get(context);
        readOption.IsOk().BeTrue();
        (groupRecord == readOption.Return()).BeTrue();

        var newGroupRecord = groupRecord with
        {
            Name = "new name"
        };

        // Use this context
        var contributorContext = groupClient.GetContext(_groupid1, _user2);

        var updateOption = await contributorContext.Set(newGroupRecord, context);
        updateOption.IsOk().BeTrue();

        readOption = await contributorContext.Get(context);
        readOption.IsOk().BeTrue();
        (newGroupRecord == readOption.Return()).BeTrue();

        var listOption = await groupClient.GroupsForPrincipalId(_user1, context);
        listOption.IsOk().BeTrue();
        listOption.Return().Action(x =>
        {
            x.Count.Be(1);
            x[0].Be(_groupid1);
        });

        var deleteOption = await contributorContext.Delete(context);
        deleteOption.IsOk().BeTrue();

        readOption = await contributorContext.Get(context);
        readOption.IsNotFound().BeTrue();
    }

    [Fact]
    public async Task UsersWithDifferentAccess()
    {
        var graphHostService = await TestHost.Create(_outputHelper);
        var context = graphHostService.CreateScopeContext<SecurityGroupClientTests>();

        var groupClient = graphHostService.Services.GetRequiredService<SecurityGroupClient>();
        var channelClient = graphHostService.Services.GetRequiredService<ChannelClient>();

        await IdentityTestTool.AddIdentityUser(_user1, "user 1", graphHostService, context);
        await IdentityTestTool.AddIdentityUser(_user2, "user 2", graphHostService, context);
        await IdentityTestTool.AddIdentityUser(_user3, "user 3", graphHostService, context);
        await IdentityTestTool.AddIdentityUser(_user4, "user 4", graphHostService, context);

        var groupRecord = CreateGroup(_groupid1, "group 1", [(_user1, SecurityAccess.Reader), (_user3, SecurityAccess.Owner), (_user4, SecurityAccess.Contributor)]);

        var addResult = await groupClient.Create(groupRecord, context);
        addResult.IsOk().BeTrue();

        (await groupClient.GetContext(_groupid1, _user2).Get(context)).IsUnauthorized().BeTrue();
        (await groupClient.GetContext(_groupid1, _user4).Get(context)).IsOk().BeTrue();

        var readOption = await groupClient.GetContext(_groupid1, _user1).Get(context);
        readOption.IsOk().BeTrue();
        (groupRecord == readOption.Return()).BeTrue();

        var newGroupRecord = groupRecord with
        {
            Name = "new name"
        };

        (await groupClient.GetContext(_groupid1, _user1).Set(newGroupRecord, context)).IsForbidden().BeTrue();
        (await groupClient.GetContext(_groupid1, _user2).Set(newGroupRecord, context)).IsUnauthorized().BeTrue();
        (await groupClient.GetContext(_groupid1, _user3).Set(newGroupRecord, context)).IsOk().BeTrue();
        (await groupClient.GetContext(_groupid1, _user4).Set(newGroupRecord, context)).IsOk().BeTrue();

        var x = await groupClient.GetContext(_groupid1, _user4).SetAccess(_user1, SecurityAccess.Owner, context);
        (await groupClient.GetContext(_groupid1, _user4).SetAccess(_user1, SecurityAccess.Owner, context)).IsForbidden().BeTrue();
        (await groupClient.GetContext(_groupid1, _user3).SetAccess(_user1, SecurityAccess.Owner, context)).IsOk().BeTrue();
        (await groupClient.GetContext(_groupid1, _user1).Set(newGroupRecord, context)).IsOk().BeTrue();

        (await groupClient.GetContext(_groupid1, _user4).DeleteAccess(_user1, context)).IsForbidden().BeTrue();
        (await groupClient.GetContext(_groupid1, _user3).DeleteAccess(_user1, context)).IsOk().BeTrue();
    }

    [Fact]
    public async Task PrincipalGroupSearch()
    {
        var graphHostService = await TestHost.Create(_outputHelper);
        var context = graphHostService.CreateScopeContext<SecurityGroupClientTests>();

        var groupClient = graphHostService.Services.GetRequiredService<SecurityGroupClient>();
        var channelClient = graphHostService.Services.GetRequiredService<ChannelClient>();

        await IdentityTestTool.AddIdentityUser(_user1, "user 1", graphHostService, context);
        await IdentityTestTool.AddIdentityUser(_user2, "user 2", graphHostService, context);
        await IdentityTestTool.AddIdentityUser(_user3, "user 3", graphHostService, context);
        await IdentityTestTool.AddIdentityUser(_user4, "user 4", graphHostService, context);

        await CreateGroup(_groupid1, "group 1", [(_user1, SecurityAccess.Reader), (_user2, SecurityAccess.Owner)]).Func(async x =>
        {
            var result = await groupClient.Create(x, context);
            result.IsOk().BeTrue();
        });

        await CreateGroup(_groupid2, "group 2", [(_user1, SecurityAccess.Reader), (_user3, SecurityAccess.Owner)]).Func(async x =>
        {
            var addResult1 = await groupClient.Create(x, context);
            addResult1.IsOk().BeTrue();
        });

        (await groupClient.GroupsForPrincipalId(_user1, context)).Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().Action(y =>
            {
                y.Count.Be(2);
                y.OrderBy(x => x).SequenceEqual([_groupid1, _groupid2]).BeTrue();
            });
        });

        (await groupClient.GroupsForPrincipalId(_user2, context)).Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().Action(y =>
            {
                y.Count.Be(1);
                y.OrderBy(x => x).SequenceEqual([_groupid1]).BeTrue();
            });
        });

        (await groupClient.GroupsForPrincipalId(_user3, context)).Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().Action(y =>
            {
                y.Count.Be(1);
                y.OrderBy(x => x).SequenceEqual([_groupid2]).BeTrue();
            });
        });

        (await groupClient.GroupsForPrincipalId(_user4, context)).Action(x =>
        {
            x.IsNotFound().BeTrue();
        });
    }

    private SecurityGroupRecord CreateGroup(string groupId, string name, IEnumerable<(string principalId, SecurityAccess access)> members)
    {
        var rec = new SecurityGroupRecord
        {
            SecurityGroupId = groupId,
            Name = name,
            Members = members.NotNull()
                .Select(x => new PrincipalAccess { PrincipalId = x.principalId, Access = x.access })
                .ToDictionary(x => x.PrincipalId),
        };

        return rec;
    }
}
