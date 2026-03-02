using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Graph.Grant;

public class GrantPolicyTests
{
    [Fact]
    public void CreateEmpty()
    {
        var policy = new GrantPolicy("key1", []);
        policy.NodeKey.Be("grantpolicy:key1");
        policy.PrincipalIdentifiers.Count.Be(0);
    }

    [Fact]
    public void CreateWithGrants()
    {
        var grants = new[]
        {
            new PrincipalGrant(RolePolicy.Reader, "principal1"),
            new PrincipalGrant(RolePolicy.Owner, "principal2"),
        };

        var policy = new GrantPolicy("key1", grants);

        policy.NodeKey.Be("grantpolicy:key1");
        policy.PrincipalIdentifiers.Count.Be(2);
        policy.PrincipalIdentifiers["principal1"].Role.Be(RolePolicy.Reader);
        policy.PrincipalIdentifiers["principal2"].Role.Be(RolePolicy.Owner);
        policy.Validate().IsOk().BeTrue();

        var json = policy.ToJson();
        var read = json.ToObject<GrantPolicy>();
        read.NotNull();
        policy.Be(read);
        (policy == read).BeTrue();
    }

    [Fact]
    public void JsonConstructorWithDictionary()
    {
        var grants = new Dictionary<string, PrincipalGrant>
        {
            ["principal1"] = new PrincipalGrant(RolePolicy.Contributor, "principal1"),
        };

        var policy = new GrantPolicy("key2", grants);

        policy.NodeKey.Be("grantpolicy:key2");
        policy.PrincipalIdentifiers.Count.Be(1);
        policy.PrincipalIdentifiers["principal1"].Role.Be(RolePolicy.Contributor);
    }

    [Fact]
    public void PrincipalGrantValidation()
    {
        Verify.Throws<ArgumentException>(() => new PrincipalGrant(RolePolicy.None, "principal1"));
        Verify.Throws<ArgumentException>(() => new PrincipalGrant(RolePolicy.Reader, ""));
    }

    [Fact]
    public void AddOrUpdateWith_AddsAndUpdates()
    {
        var policy = new GrantPolicy("key1", new[] { new PrincipalGrant(RolePolicy.Reader, "principal1") });

        var updated = policy.AddOrUpdateWith(RolePolicy.Owner, new[] { "principal1", "principal2" });

        updated.NodeKey.Be("grantpolicy:key1");
        updated.PrincipalIdentifiers.Count.Be(2);
        updated.PrincipalIdentifiers["principal1"].Role.Be(RolePolicy.Owner);
        updated.PrincipalIdentifiers["principal2"].Role.Be(RolePolicy.Owner);
        policy.PrincipalIdentifiers["principal1"].Role.Be(RolePolicy.Reader);
    }

    [Fact]
    public void RemoveWith_RemovesSpecifiedPrincipals()
    {
        var policy = new GrantPolicy("key1", new[]
        {
            new PrincipalGrant(RolePolicy.Reader, "principal1"),
            new PrincipalGrant(RolePolicy.Reader, "principal2"),
        });

        var updated = policy.RemoveWith(new[] { "principal1" });

        updated.PrincipalIdentifiers.Count.Be(1);
        updated.PrincipalIdentifiers.ContainsKey("principal2").BeTrue();
        updated.NodeKey.Be("grantpolicy:key1");
        policy.PrincipalIdentifiers.Count.Be(2);
    }

    [Fact]
    public void RemoveWith_NoMatchingPrincipals_ReturnsOriginalSet()
    {
        var policy = new GrantPolicy("key1", new[]
        {
            new PrincipalGrant(RolePolicy.Reader, "principal1"),
        });

        var updated = policy.RemoveWith(new[] { "other" });

        updated.PrincipalIdentifiers.Count.Be(1);
        updated.PrincipalIdentifiers.ContainsKey("principal1").BeTrue();
    }

    [Fact]
    public void PermissionChecks_CanReadWriteAndOwner()
    {
        var policy = new GrantPolicy("key1", new[]
        {
            new PrincipalGrant(RolePolicy.Reader, "reader"),
            new PrincipalGrant(RolePolicy.Contributor, "contrib"),
            new PrincipalGrant(RolePolicy.Owner, "owner"),
        });

        policy.CanRead("reader").BeTrue();
        policy.CanRead("contrib").BeTrue();
        policy.CanRead("owner").BeTrue();
        policy.CanRead("unknown").BeFalse();

        policy.CanWrite("reader").BeFalse();
        policy.CanWrite("contrib").BeTrue();
        policy.CanWrite("owner").BeTrue();
        policy.CanWrite("unknown").BeFalse();

        policy.IsOwner("owner").BeTrue();
        policy.IsOwner("reader").BeFalse();
        policy.IsOwner("contrib").BeFalse();
        policy.IsOwner("unknown").BeFalse();
    }

    [Fact]
    public void AddOrUpdateWith_NullPrincipalIdentifiers_Throws()
    {
        var policy = new GrantPolicy("key1", new[] { new PrincipalGrant(RolePolicy.Reader, "principal1") });
        Verify.Throws<ArgumentNullException>(() => policy.AddOrUpdateWith(RolePolicy.Reader, null!));
    }

    [Fact]
    public void AddOrUpdateWith_EmptyPrincipal_Throws()
    {
        var policy = new GrantPolicy("key1", new[] { new PrincipalGrant(RolePolicy.Reader, "principal1") });
        Verify.Throws<ArgumentException>(() => policy.AddOrUpdateWith(RolePolicy.Reader, new[] { "" }));
    }

    [Fact]
    public void RemoveWith_NullPrincipalIdentifiers_Throws()
    {
        var policy = new GrantPolicy("key1", new[] { new PrincipalGrant(RolePolicy.Reader, "principal1") });
        Verify.Throws<ArgumentNullException>(() => policy.RemoveWith(null!));
    }

    [Fact]
    public void EqualityChecks()
    {
        var policy1 = new GrantPolicy("key1", new[] { new PrincipalGrant(RolePolicy.Reader, "principal1") });
        var policy2 = new GrantPolicy("key1", new[] { new PrincipalGrant(RolePolicy.Reader, "principal1") });
        var policy3 = new GrantPolicy("key2", new[] { new PrincipalGrant(RolePolicy.Reader, "principal1") });
        var policy4 = new GrantPolicy("key1", new[] { new PrincipalGrant(RolePolicy.Owner, "principal1") });

        (policy1 == policy2).BeTrue();
        policy1.Equals(policy2).BeTrue();
        policy1.Equals(policy3).BeFalse();
        policy1.Equals(policy4).BeFalse();
    }

    [Fact]
    public void CreateKey_Static()
    {
        var key = GrantPolicy.CreateKey("node1");
        key.Be("grantpolicy/node1");
    }
}
