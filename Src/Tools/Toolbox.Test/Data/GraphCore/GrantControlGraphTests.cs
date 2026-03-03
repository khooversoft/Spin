using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Data.Graph;

public class GrantControlGraphTests
{
    [Fact]
    public void SingleNode()
    {
        var graph = new GraphCore();

        var group1 = new GroupDetail("groupName");
        var node = new Node(group1.Name, group1.ToDataETag());
        graph.Nodes.TryAdd(node).ThrowOnError();

        graph.Nodes.TryGetValue(group1.Name, out var value).Action(x =>
        {
            x.BeTrue();
            value.NotNull();
            value.Be(node);
            value.Payload.NotNull().ToObject<GroupDetail>().Be(group1);
        });
    }

    [Fact]
    public void EdgeRequiresExistingNodes()
    {
        const string edgeType = "memberOf";
        var graph = new GraphCore();

        var group = new GroupDetail("groupName");
        graph.Nodes.Add(group.Name, group.ToDataETag()).ThrowOnError();

        graph.Edges.Add("missingUser", group.Name, edgeType).BeNotFound();

        var principal = new PrincipalIdentity("principalId", "nameIdentifier", "userName", "email");
        graph.Nodes.Add(principal.PrincipalId, principal.ToDataETag()).ThrowOnError();

        graph.Edges.Add(principal.PrincipalId, "missingGroup", edgeType).BeNotFound();
        graph.Edges.GetByType(edgeType).Any().BeFalse();
    }

    [Fact]
    public void GroupToUser()
    {
        const string edgeType = "memberOf";
        var graph = new GraphCore();

        var group = new GroupDetail("groupName");
        graph.Nodes.Add(group.Name, group.ToDataETag()).ThrowOnError();

        var principal = new PrincipalIdentity("principalId", "nameIdentifier", "userName", "email");
        graph.Nodes.Add(principal.PrincipalId, principal.ToDataETag()).ThrowOnError();

        graph.Edges.Add(principal.PrincipalId, group.Name, edgeType).ThrowOnError();

        // Groups -> users
        var userInGroup = graph.Nodes.GetNodes(group.Name)
            .SelectMany(x => graph.Edges.GetByTo(x.NodeKey).Where(x => x.EdgeType == edgeType))
            .SelectMany(x => graph.Nodes.GetNodes(x.FromKey))
            .ToArray();
        userInGroup.Length.Be(1);
        userInGroup[0].Payload.ToObject<PrincipalIdentity>().Be(principal);

        // Users => Groups
        var groupsForUser = graph.Nodes.GetNodes(principal.PrincipalId)
            .SelectMany(x => graph.Edges.GetByFrom(x.NodeKey).Where(x => x.EdgeType == edgeType))
            .SelectMany(x => graph.Nodes.GetNodes(x.ToKey))
            .ToArray();
        groupsForUser.Length.Be(1);
        groupsForUser[0].Payload.ToObject<GroupDetail>().Be(group);
    }

    [Fact]
    public void DuplicateMembershipEdgeShouldReturnConflict()
    {
        const string edgeType = "memberOf";
        var graph = new GraphCore();

        var group = new GroupDetail("groupName");
        graph.Nodes.Add(group.Name, group.ToDataETag()).ThrowOnError();

        var principal = new PrincipalIdentity("principalId", "nameIdentifier", "userName", "email");
        graph.Nodes.Add(principal.PrincipalId, principal.ToDataETag()).ThrowOnError();

        graph.Edges.Add(principal.PrincipalId, group.Name, edgeType).ThrowOnError();

        graph.Edges.Add(principal.PrincipalId, group.Name, edgeType).BeConflict();
        graph.Edges.GetByFrom(principal.PrincipalId).Count.Be(1);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(3, 2)]
    [InlineData(2, 3)]
    [InlineData(4, 10)]
    [InlineData(10, 4)]
    public void DifferentGroupToUser(int groupCount, int userCount)
    {
        const string edgeType = "memberOf";

        var graph = new GraphCore();

        var groups = Enumerable.Range(0, groupCount)
            .Select(x => new GroupDetail($"groupName-{x}"))
            .ForEach(x => graph.Nodes.Add(x.Name, x.ToDataETag()).ThrowOnError());

        var users = Enumerable.Range(0, userCount)
            .Select(x => new PrincipalIdentity($"principalId-{x}", $"nameIdentifier-{x}", $"userName-{x}", $"email-{x}"))
            .ForEach(x => graph.Nodes.Add(x.PrincipalId, x.ToDataETag()).ThrowOnError());

        foreach (var user in users)
        {
            foreach (var group in groups)
            {
                // From (user) -> To (group)
                graph.Edges.Add(user.PrincipalId, group.Name, edgeType).ThrowOnError();
            }
        }

        foreach (var group in groups)
        {
            // Groups -> users
            var groupToUser = graph.Nodes.GetNodes(group.Name)
                .SelectMany(x => graph.Edges.GetByTo(x.NodeKey).Where(x => x.EdgeType == edgeType))
                .SelectMany(x => graph.Nodes.GetNodes(x.FromKey))
                .ToArray();
            groupToUser.Length.Be(userCount);

            var objs = groupToUser
                .Select(x => x.Payload.NotNull().ToObject<PrincipalIdentity>())
                .OrderBy(x => x.PrincipalId)
                .ToArray();

            var test = objs.OrderBy(x => x.PrincipalId).Zip(users);
            test.All(x => x.First == x.Second).BeTrue();
        }

        foreach (var user in users)
        {
            // Users => Group
            var userToGroup = graph.Nodes.GetNodes(user.PrincipalId)
                .SelectMany(x => graph.Edges.GetByFrom(x.NodeKey).Where(x => x.EdgeType == edgeType))
                .SelectMany(x => graph.Nodes.GetNodes(x.ToKey))
                .ToArray();
            userToGroup.Length.Be(groupCount);

            var objs = userToGroup
                .Select(x => x.Payload.NotNull().ToObject<GroupDetail>())
                .OrderBy(x => x.Name)
                .ToArray();

            var test = objs.OrderBy(x => x.Name).Zip(groups);
            test.All(x => x.First == x.Second).BeTrue();
        }
    }

    [Fact]
    public void RemovingNodesShouldRemoveMembershipEdges()
    {
        const string edgeType = "memberOf";
        var graph = new GraphCore();

        var group = new GroupDetail("groupName");
        graph.Nodes.Add(group.Name, group.ToDataETag()).ThrowOnError();

        var principal = new PrincipalIdentity("principalId", "nameIdentifier", "userName", "email");
        graph.Nodes.Add(principal.PrincipalId, principal.ToDataETag()).ThrowOnError();

        graph.Edges.Add(principal.PrincipalId, group.Name, edgeType).ThrowOnError();

        graph.Nodes.Remove(group.Name).ThrowOnError();

        graph.Edges.GetByFrom(principal.PrincipalId).Any().BeFalse();
        graph.Edges.GetByTo(group.Name).Any().BeFalse();

        graph.Nodes.Remove(principal.PrincipalId).ThrowOnError();

        graph.Edges.GetByType(edgeType).Any().BeFalse();
    }



    private record GroupDetail(string Name);
    private record GroupPolicy(string NameIdentifier, string PrincipalIdentifier);
    private record PrincipalIdentity(string PrincipalId, string NameIdentifier, string UserName, string Email);
}
