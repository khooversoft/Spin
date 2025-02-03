using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Graph.Extensions.test.Tools;
using Toolbox.Graph.Extensions.Testing;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions.test.PrincipalGroup;

public class SecurityGroupClientTests
{
    private const string _user1 = "user1@domain.com";
    private const string _user2 = "user2@domain.com";
    private const string _user3 = "user3@domain.com";
    private const string _user4 = "user4@domain.com";
    private const string _groupid1 = "groupid1";
    private const string _groupid2 = "groupid2";

    [Fact]
    public async Task Lifecycle()
    {
        var testHost = new ToolboxExtensionTestHost();
        var client = testHost.ServiceProvider.GetRequiredService<SecurityGroupClient>();
        IGraphClient graphClient = testHost.ServiceProvider.GetRequiredService<IGraphClient>();
        var context = testHost.GetScopeContext<SecurityGroupClientTests>();

        await IdentityTestTool.AddIdentityUser(_user1, "user 1", testHost, context);
        await IdentityTestTool.AddIdentityUser(_user2, "user 2", testHost, context);

        var groupRecord = CreateGroup(_groupid1, "group 1", [(_user1, SecurityAccess.Reader), (_user2, SecurityAccess.Owner)]);

        var addResult = await client.Create(groupRecord, context);
        addResult.IsOk().Should().BeTrue();

        var readOption = await client.GetContext(_groupid1, _user1).Get(context);
        readOption.IsOk().Should().BeTrue();
        (groupRecord == readOption.Return()).Should().BeTrue();

        var newGroupRecord = groupRecord with
        {
            Name = "new name"
        };

        var updateOption = await client.GetContext(_groupid1, _user2).Set(newGroupRecord, context);
        updateOption.IsOk().Should().BeTrue();

        readOption = await client.GetContext(_groupid1, _user2).Get(context);
        readOption.IsOk().Should().BeTrue();
        (newGroupRecord == readOption.Return()).Should().BeTrue();

        var listOption = await client.GroupsForPrincipalId(_user1, context);
        listOption.IsOk().Should().BeTrue();
        listOption.Return().Action(x =>
        {
            x.Count.Should().Be(1);
            x[0].Should().Be(_groupid1);
        });

        var deleteOption = await client.GetContext(_groupid1, _user2).Delete(context);
        deleteOption.IsOk().Should().BeTrue();

        readOption = await client.GetContext(_groupid1, _user2).Get(context);
        readOption.IsNotFound().Should().BeTrue();
    }

    [Fact]
    public async Task LifecycleWithContext()
    {
        var testHost = new ToolboxExtensionTestHost();
        var client = testHost.ServiceProvider.GetRequiredService<SecurityGroupClient>();
        IGraphClient graphClient = testHost.ServiceProvider.GetRequiredService<IGraphClient>();
        var context = testHost.GetScopeContext<SecurityGroupClientTests>();

        await IdentityTestTool.AddIdentityUser(_user1, "user 1", testHost, context);
        await IdentityTestTool.AddIdentityUser(_user2, "user 2", testHost, context);

        var groupRecord = CreateGroup(_groupid1, "group 1", [(_user1, SecurityAccess.Reader), (_user2, SecurityAccess.Owner)]);

        var addResult = await client.Create(groupRecord, context);
        addResult.IsOk().Should().BeTrue();

        var readOption = await client.GetContext(_groupid1, _user1).Get(context);
        readOption.IsOk().Should().BeTrue();
        (groupRecord == readOption.Return()).Should().BeTrue();

        var newGroupRecord = groupRecord with
        {
            Name = "new name"
        };

        // Use this context
        var contributorContext = client.GetContext(_groupid1, _user2);

        var updateOption = await contributorContext.Set(newGroupRecord, context);
        updateOption.IsOk().Should().BeTrue();

        readOption = await contributorContext.Get(context);
        readOption.IsOk().Should().BeTrue();
        (newGroupRecord == readOption.Return()).Should().BeTrue();

        var listOption = await client.GroupsForPrincipalId(_user1, context);
        listOption.IsOk().Should().BeTrue();
        listOption.Return().Action(x =>
        {
            x.Count.Should().Be(1);
            x[0].Should().Be(_groupid1);
        });

        var deleteOption = await contributorContext.Delete(context);
        deleteOption.IsOk().Should().BeTrue();

        readOption = await contributorContext.Get(context);
        readOption.IsNotFound().Should().BeTrue();
    }

    [Fact]
    public async Task UsersWithDifferentAccess()
    {
        var testHost = new ToolboxExtensionTestHost();
        var client = testHost.ServiceProvider.GetRequiredService<SecurityGroupClient>();
        IGraphClient graphClient = testHost.ServiceProvider.GetRequiredService<IGraphClient>();
        var context = testHost.GetScopeContext<SecurityGroupClientTests>();

        await IdentityTestTool.AddIdentityUser(_user1, "user 1", testHost, context);
        await IdentityTestTool.AddIdentityUser(_user2, "user 2", testHost, context);
        await IdentityTestTool.AddIdentityUser(_user3, "user 3", testHost, context);
        await IdentityTestTool.AddIdentityUser(_user4, "user 4", testHost, context);

        var groupRecord = CreateGroup(_groupid1, "group 1", [(_user1, SecurityAccess.Reader), (_user3, SecurityAccess.Owner), (_user4, SecurityAccess.Contributor)]);

        var addResult = await client.Create(groupRecord, context);
        addResult.IsOk().Should().BeTrue();

        (await client.GetContext(_groupid1, _user2).Get(context)).IsUnauthorized().Should().BeTrue();
        (await client.GetContext(_groupid1, _user4).Get(context)).IsOk().Should().BeTrue();

        var readOption = await client.GetContext(_groupid1, _user1).Get(context);
        readOption.IsOk().Should().BeTrue();
        (groupRecord == readOption.Return()).Should().BeTrue();

        var newGroupRecord = groupRecord with
        {
            Name = "new name"
        };

        (await client.GetContext(_groupid1, _user1).Set(newGroupRecord, context)).IsForbidden().Should().BeTrue();
        (await client.GetContext(_groupid1, _user2).Set(newGroupRecord, context)).IsUnauthorized().Should().BeTrue();
        (await client.GetContext(_groupid1, _user3).Set(newGroupRecord, context)).IsOk().Should().BeTrue();
        (await client.GetContext(_groupid1, _user4).Set(newGroupRecord, context)).IsOk().Should().BeTrue();

        var x = await client.GetContext(_groupid1, _user4).SetAccess(_user1, SecurityAccess.Owner, context);
        (await client.GetContext(_groupid1, _user4).SetAccess(_user1, SecurityAccess.Owner, context)).IsForbidden().Should().BeTrue();
        (await client.GetContext(_groupid1, _user3).SetAccess(_user1, SecurityAccess.Owner, context)).IsOk().Should().BeTrue();
        (await client.GetContext(_groupid1, _user1).Set(newGroupRecord, context)).IsOk().Should().BeTrue();

        (await client.GetContext(_groupid1, _user4).DeleteAccess(_user1, context)).IsForbidden().Should().BeTrue();
        (await client.GetContext(_groupid1, _user3).DeleteAccess(_user1, context)).IsOk().Should().BeTrue();
    }

    [Fact]
    public async Task PrincipalGroupSearch()
    {
        var testHost = new ToolboxExtensionTestHost();
        var client = testHost.ServiceProvider.GetRequiredService<SecurityGroupClient>();
        IGraphClient graphClient = testHost.ServiceProvider.GetRequiredService<IGraphClient>();
        var context = testHost.GetScopeContext<SecurityGroupClientTests>();

        await IdentityTestTool.AddIdentityUser(_user1, "user 1", testHost, context);
        await IdentityTestTool.AddIdentityUser(_user2, "user 2", testHost, context);
        await IdentityTestTool.AddIdentityUser(_user3, "user 3", testHost, context);
        await IdentityTestTool.AddIdentityUser(_user4, "user 4", testHost, context);

        await CreateGroup(_groupid1, "group 1", [(_user1, SecurityAccess.Reader), (_user2, SecurityAccess.Owner)]).Func(async x =>
        {
            var result = await client.Create(x, context);
            result.IsOk().Should().BeTrue();
        });

        await CreateGroup(_groupid2, "group 2", [(_user1, SecurityAccess.Reader), (_user3, SecurityAccess.Owner)]).Func(async x =>
        {
            var addResult1 = await client.Create(x, context);
            addResult1.IsOk().Should().BeTrue();
        });

        (await client.GroupsForPrincipalId(_user1, context)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Action(y =>
            {
                y.Count.Should().Be(2);
                y.OrderBy(x => x).SequenceEqual([_groupid1, _groupid2]).Should().BeTrue();
            });
        });

        (await client.GroupsForPrincipalId(_user2, context)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Action(y =>
            {
                y.Count.Should().Be(1);
                y.OrderBy(x => x).SequenceEqual([_groupid1]).Should().BeTrue();
            });
        });

        (await client.GroupsForPrincipalId(_user3, context)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Action(y =>
            {
                y.Count.Should().Be(1);
                y.OrderBy(x => x).SequenceEqual([_groupid2]).Should().BeTrue();
            });
        });

        (await client.GroupsForPrincipalId(_user4, context)).Action(x =>
        {
            x.IsNotFound().Should().BeTrue();
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
