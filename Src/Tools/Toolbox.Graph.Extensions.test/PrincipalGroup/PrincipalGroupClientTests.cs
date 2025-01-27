using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Graph.Extensions.test.Tools;
using Toolbox.Graph.Extensions.Testing;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions.test.PrincipalGroup;

public class PrincipalGroupClientTests
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
        var context = testHost.GetScopeContext<PrincipalGroupClientTests>();

        await IdentityTestTool.AddIdentityUser(_user1, "user 1", testHost, context);
        await IdentityTestTool.AddIdentityUser(_user2, "user 2", testHost, context);

        var groupRecord = CreateGroup(_groupid1, "group 1", [(_user1, PrincipalAccess.Read), (_user2, PrincipalAccess.Contributor)]);

        var addResult = await client.Add(groupRecord, context);
        addResult.IsOk().Should().BeTrue();

        var readOption = await client.Get(_groupid1, context);
        readOption.IsOk().Should().BeTrue();
        (groupRecord == readOption.Return()).Should().BeTrue();

        var newGroupRecord = groupRecord with
        {
            Name = "new name"
        };

        var updateOption = await client.Set(newGroupRecord, context);
        updateOption.IsOk().Should().BeTrue();

        readOption = await client.Get(_groupid1, context);
        readOption.IsOk().Should().BeTrue();
        (newGroupRecord == readOption.Return()).Should().BeTrue();

        var listOption = await client.GroupsForPrincipalId(_user1, context);
        listOption.IsOk().Should().BeTrue();
        listOption.Return().Action(x =>
        {
            x.Count.Should().Be(1);
            x[0].Should().Be(_groupid1);
        });

        var deleteOption = await client.Delete(_groupid1, context);
        deleteOption.IsOk().Should().BeTrue();

        readOption = await client.Get(_groupid1, context);
        readOption.IsNotFound().Should().BeTrue();
    }

    [Fact]
    public async Task PrincipalGroupSearch()
    {
        var testHost = new ToolboxExtensionTestHost();
        var client = testHost.ServiceProvider.GetRequiredService<SecurityGroupClient>();
        IGraphClient graphClient = testHost.ServiceProvider.GetRequiredService<IGraphClient>();
        var context = testHost.GetScopeContext<PrincipalGroupClientTests>();

        await IdentityTestTool.AddIdentityUser(_user1, "user 1", testHost, context);
        await IdentityTestTool.AddIdentityUser(_user2, "user 2", testHost, context);
        await IdentityTestTool.AddIdentityUser(_user3, "user 3", testHost, context);
        await IdentityTestTool.AddIdentityUser(_user4, "user 4", testHost, context);

        await CreateGroup(_groupid1, "group 1", [(_user1, PrincipalAccess.Read), (_user2, PrincipalAccess.Contributor)]).Func(async x =>
        {
            var addResult1 = await client.Add(x, context);
            addResult1.IsOk().Should().BeTrue();
        });

        await CreateGroup(_groupid2, "group 2", [(_user1, PrincipalAccess.Read), (_user3, PrincipalAccess.Contributor)]).Func(async x =>
        {
            var addResult1 = await client.Add(x, context);
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

    private SecurityGroupRecord CreateGroup(string groupId, string name, IEnumerable<(string principalId, PrincipalAccess access)> members)
    {
        var rec = new SecurityGroupRecord
        {
            SecurityGroupId = groupId,
            Name = name,
            Members = members.NotNull()
                .Select(x => new MemberAccessRecord { PrincipalId = x.principalId, Access = x.access })
                .ToDictionary(x => x.PrincipalId),
        };

        return rec;
    }
}
