using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Data;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Graph.Grant;

public class GrantPolicyRegistratryTests
{
    [Fact]
    public void Empty()
    {
        var core = new GraphCore();

        var registry = new GrantPolicyRegistry(() => core, NullLogger.Instance);
        registry.NotNull();
        registry.GetAll().Count.Be(0);
        registry.Get("missing").BeNotFound();
    }

    [Fact]
    public void NoDupicatePolicies()
    {
        var core = new GraphCore();
        var registry = new GrantPolicyRegistry(() => core, NullLogger.Instance);

        Action act = () =>
        {
            var policy = new GrantPolicy("node1", new[]
            {
                new PrincipalGrant(RolePolicy.Reader, "user1"),
                new PrincipalGrant(RolePolicy.Contributor, "user2"),
                new PrincipalGrant(RolePolicy.Owner, "user2"),
            });
        };

        Verify.Throws<ArgumentException>(act);
    }

    [Fact]
    public void AddPolicy()
    {
        var core = new GraphCore();
        var registry = new GrantPolicyRegistry(() => core, NullLogger.Instance);

        var policy = new GrantPolicy("node1", new[]
        {
            new PrincipalGrant(RolePolicy.Reader, "user1"),
            new PrincipalGrant(RolePolicy.Contributor, "user2"),
            new PrincipalGrant(RolePolicy.Owner, "user3"),
        });

        registry.TryAdd(policy).BeOk();
        registry.TryAdd(policy).BeConflict();

        registry.GetAll().Count.Be(1);
        var readPolicy = registry.Get(policy.NodeKey).BeOk().Return();
        (policy == readPolicy).BeTrue();
        policy.Equals(readPolicy).BeTrue();

        readPolicy.CanRead("user1").BeTrue();
        readPolicy.CanRead("user2").BeTrue();
        readPolicy.CanRead("user3").BeTrue();
        readPolicy.CanWrite("user1").BeFalse();
        readPolicy.CanWrite("user2").BeTrue();
        readPolicy.CanWrite("user3").BeTrue();
        readPolicy.IsOwner("user1").BeFalse();
        readPolicy.IsOwner("user2").BeFalse();
        readPolicy.IsOwner("user3").BeTrue();

        registry.Remove(policy.NodeKey).BeOk();
        registry.GetAll().Count.Be(0);
    }

    [Fact]
    public void AddOrUpdate_MissingNode_ReturnsNotFound()
    {
        var core = new GraphCore();
        var registry = new GrantPolicyRegistry(() => core, NullLogger.Instance);

        var policy = new GrantPolicy("node1", new[]
        {
            new PrincipalGrant(RolePolicy.Reader, "user1"),
        });

        registry.AddOrUpdate(policy).BeNotFound();
        registry.GetAll().Count.Be(0);
    }

    [Fact]
    public void AddOrUpdate_UpdatesExistingPolicy()
    {
        var core = new GraphCore();
        var registry = new GrantPolicyRegistry(() => core, NullLogger.Instance);

        var original = new GrantPolicy("node1", new[]
        {
            new PrincipalGrant(RolePolicy.Reader, "user1"),
            new PrincipalGrant(RolePolicy.Contributor, "user2"),
        });

        registry.TryAdd(original).BeOk();

        var updated = new GrantPolicy("node1", new[]
        {
            new PrincipalGrant(RolePolicy.Owner, "user3"),
        });

        registry.AddOrUpdate(updated).BeOk();

        var stored = registry.Get(updated.NodeKey).BeOk().Return();
        stored.Be(updated);

        stored.CanRead("user1").BeFalse();
        stored.CanWrite("user2").BeFalse();
        stored.IsOwner("user3").BeTrue();
    }

    [Fact]
    public void Get_ResolvesNodeKeyWithoutType()
    {
        var core = new GraphCore();
        var registry = new GrantPolicyRegistry(() => core, NullLogger.Instance);

        var policy = new GrantPolicy("node1", new[]
        {
            new PrincipalGrant(RolePolicy.Reader, "user1"),
        });

        registry.TryAdd(policy).BeOk();

        var stored = registry.Get("node1").BeOk().Return();
        stored.NodeKey.Be(policy.NodeKey);
        stored.Be(policy);
    }

    [Fact]
    public void GetAll_FiltersGrantPolicies()
    {
        var core = new GraphCore();
        var registry = new GrantPolicyRegistry(() => core, NullLogger.Instance);

        var policy1 = new GrantPolicy("node1", new[] { new PrincipalGrant(RolePolicy.Reader, "user1") });
        var policy2 = new GrantPolicy("node2", new[] { new PrincipalGrant(RolePolicy.Owner, "user2") });

        registry.TryAdd(policy1).BeOk();
        registry.TryAdd(policy2).BeOk();

        var unrelatedNode = new Node(NodeTool.CreateKey("other", "custom"), new DataETag("value"u8.ToArray()));
        core.Nodes.TryAdd(unrelatedNode).BeOk();

        var policies = registry.GetAll().OrderBy(x => x.NodeKey).ToArray();

        policies.Length.Be(2);
        policies[0].Be(policy1);
        policies[1].Be(policy2);
        core.Nodes.ContainsKey(unrelatedNode.NodeKey).BeTrue();
    }

    [Fact]
    public void Remove_MissingPolicy_ReturnsNotFound()
    {
        var core = new GraphCore();
        var registry = new GrantPolicyRegistry(() => core, NullLogger.Instance);

        var otherNode = new Node(NodeTool.CreateKey("other", "custom"), new DataETag("value"u8.ToArray()));
        core.Nodes.TryAdd(otherNode).BeOk();

        registry.Remove("missing").BeNotFound();

        core.Nodes.ContainsKey(otherNode.NodeKey).BeTrue();
        registry.GetAll().Count.Be(0);
    }
}
