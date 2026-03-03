using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Graph.Grant;

public class PrincipalRegistryTests
{
    [Fact]
    public void Empty()
    {
        var core = new GraphCore();

        var registry = new PrincipalRegistry(() => core, NullLogger.Instance);
        registry.NotNull();
        registry.GetAll().Count.Be(0);
        registry.Get("missing").BeNotFound();
    }

    [Fact]
    public void AddOrUpdate_AddsPrincipalAndReferenceNodes()
    {
        var core = new GraphCore();
        var registry = new PrincipalRegistry(() => core, NullLogger.Instance);

        var principal = new PrincipalIdentity("user1", "nameId1", "userName1", "email1@domain.com", true);

        registry.AddOrUpdate(principal).BeOk();

        registry.Get(principal.PrincipalId).BeOk();

        var stored = registry.Get(principal.PrincipalId).BeOk();
        stored.HasValue.BeTrue();
        stored.Value.Be(principal);

        var all = registry.GetAll();
        all.Count.Be(1);
        all[0].Be(principal);

        core.Nodes.ContainsKey(principal.NodeKey).BeTrue();
        core.Nodes.ContainsKey(principal.CreateNameIdentifierNodeKey()).BeTrue();
        core.Nodes.ContainsKey(principal.CreateUserNameNodeKey()).BeTrue();
        core.Nodes.ContainsKey(principal.CreateEmailNodeKey()).BeTrue();

        var edges = core.Edges.GetByFrom(principal.NodeKey, PrincipalIdentity.NodeReferenceType);
        edges.Count.Be(3);


        var source = edges.Select(x => x.ToKey).OrderBy(x => x).ToArray();
        var shouldBe = new[]
        {
            principal.CreateEmailNodeKey(),
            principal.CreateNameIdentifierNodeKey(),
            principal.CreateUserNameNodeKey(),
        }.OrderBy(x => x).ToArray();

        source.SequenceEqual(shouldBe).BeTrue();
    }

    [Fact]
    public void AddOrUpdate_UpdatePrincipal_ReplacesReferenceNodes()
    {
        var core = new GraphCore();
        var registry = new PrincipalRegistry(() => core, NullLogger.Instance);

        var original = new PrincipalIdentity("user1", "nameId1", "userName1", "email1@domain.com", false);
        registry.TryAdd(original).BeOk();

        var oldNameKey = original.CreateNameIdentifierNodeKey();
        var oldUserKey = original.CreateUserNameNodeKey();
        var oldEmailKey = original.CreateEmailNodeKey();

        var updated = new PrincipalIdentity("user1", "nameId2", "userName2", "email2@domain.com", true);
        registry.AddOrUpdate(updated).BeOk();

        var stored = registry.Get(updated.PrincipalId).BeOk();
        stored.Value.Be(updated);

        core.Nodes.ContainsKey(updated.NodeKey).BeTrue();
        core.Nodes.ContainsKey(updated.CreateNameIdentifierNodeKey()).BeTrue();
        core.Nodes.ContainsKey(updated.CreateUserNameNodeKey()).BeTrue();
        core.Nodes.ContainsKey(updated.CreateEmailNodeKey()).BeTrue();

        core.Nodes.ContainsKey(oldNameKey).BeFalse();
        core.Nodes.ContainsKey(oldUserKey).BeFalse();
        core.Nodes.ContainsKey(oldEmailKey).BeFalse();

        var edges = core.Edges.GetByFrom(updated.NodeKey, PrincipalIdentity.NodeReferenceType);
        edges.Count.Be(3);

        var a1 = new[]
        {
            updated.CreateEmailNodeKey(),
            updated.CreateNameIdentifierNodeKey(),
            updated.CreateUserNameNodeKey(),
        }.OrderBy(x => x).ToArray();

        edges.Select(x => x.ToKey).OrderBy(x => x).ToArray().Be(a1);
    }

    [Fact]
    public void Remove_RemovesPrincipalAndReferenceNodes()
    {
        var core = new GraphCore();
        var registry = new PrincipalRegistry(() => core, NullLogger.Instance);

        var principal = new PrincipalIdentity("user1", "nameId1", "userName1", "email1@domain.com", false);
        registry.AddOrUpdate(principal).BeOk();

        registry.Remove(principal.PrincipalId).BeOk();

        registry.Get(principal.PrincipalId).BeNotFound();

        core.Nodes.ContainsKey(principal.NodeKey).BeFalse();
        core.Nodes.ContainsKey(principal.CreateNameIdentifierNodeKey()).BeFalse();
        core.Nodes.ContainsKey(principal.CreateUserNameNodeKey()).BeFalse();
        core.Nodes.ContainsKey(principal.CreateEmailNodeKey()).BeFalse();

        core.Edges.GetByFrom(principal.NodeKey, PrincipalIdentity.NodeReferenceType).Count.Be(0);
    }

    [Fact]
    public void Remove_MissingPrincipal_ReturnsNotFound()
    {
        var core = new GraphCore();
        var registry = new PrincipalRegistry(() => core, NullLogger.Instance);

        var otherNode = new Node(NodeTool.CreateKey("otherUser", "other"), new DataETag("payload".ToBytes()));
        core.Nodes.Add(otherNode.NodeKey, otherNode.Payload).BeOk();

        registry.Remove("missing").BeNotFound();

        core.Nodes.ContainsKey(otherNode.NodeKey).BeTrue();
        core.Nodes.Count.Be(1);
    }

    [Fact]
    public void TryAdd_ReturnsConflict()
    {
        var core = new GraphCore();
        var registry = new PrincipalRegistry(() => core, NullLogger.Instance);

        var principal = new PrincipalIdentity("user1", "nameId1", "userName1", "email1@domain.com", false);
        registry.TryAdd(principal).BeOk();

        registry.TryAdd(principal).BeConflict();

        registry.AddOrUpdate(principal).BeOk();
        registry.TryAdd(principal).BeConflict();
    }

    [Fact]
    public void TryAdd_AddsPrincipalAndReferenceNodes()
    {
        var core = new GraphCore();
        var registry = new PrincipalRegistry(() => core, NullLogger.Instance);

        var principal = new PrincipalIdentity("user1", "nameId1", "userName1", "email1@domain.com", true);

        registry.TryAdd(principal).BeOk();

        registry.Get(principal.PrincipalId).BeOk();

        var stored = registry.Get(principal.PrincipalId).BeOk();
        stored.Value.Be(principal);

        var edges = core.Edges.GetByFrom(principal.NodeKey, PrincipalIdentity.NodeReferenceType);
        edges.Count.Be(3);

        var expected = new[]
        {
            principal.CreateEmailNodeKey(),
            principal.CreateNameIdentifierNodeKey(),
            principal.CreateUserNameNodeKey(),
        }.OrderBy(x => x).ToArray();

        edges.Select(x => x.ToKey).OrderBy(x => x).ToArray().Be(expected);
    }

    [Fact]
    public void GetAll_ReturnsAllPrincipals()
    {
        var core = new GraphCore();
        var registry = new PrincipalRegistry(() => core, NullLogger.Instance);

        var principal1 = new PrincipalIdentity("user1", "nameId1", "userName1", "email1@domain.com", false);
        var principal2 = new PrincipalIdentity("user2", "nameId2", "userName2", "email2@domain.com", true);

        registry.AddOrUpdate(principal1).BeOk();
        registry.AddOrUpdate(principal2).BeOk();

        var all = registry.GetAll().OrderBy(x => x.PrincipalId).ToArray();
        all.Length.Be(2);
        all[0].Be(principal1);
        all[1].Be(principal2);
    }

    [Fact]
    public void GetAll_IgnoresNonPrincipalNodes()
    {
        var core = new GraphCore();
        var registry = new PrincipalRegistry(() => core, NullLogger.Instance);

        var principal = new PrincipalIdentity("user1", "nameId1", "userName1", "email1@domain.com", false);
        registry.AddOrUpdate(principal).BeOk();

        var unrelatedNode = new Node(NodeTool.CreateKey("other", "custom"), new DataETag("value".ToBytes()));
        core.Nodes.Add(unrelatedNode.NodeKey, unrelatedNode.Payload).BeOk();

        var principals = registry.GetAll().ToArray();

        principals.Length.Be(1);
        principals[0].Be(principal);
        core.Nodes.ContainsKey(unrelatedNode.NodeKey).BeTrue();
    }

    [Fact]
    public void Serialization_RoundTripsPrincipalRegistryState()
    {
        var core = new GraphCore();
        var registry = new PrincipalRegistry(() => core, NullLogger.Instance);

        var principals = new[]
        {
            new PrincipalIdentity("user1", "nameId1", "userName1", "email1@domain.com", true),
            new PrincipalIdentity("user2", "nameId2", "userName2", "email2@domain.com", false),
        };

        foreach (var principal in principals) registry.AddOrUpdate(principal).BeOk();

        var json = core.ToJson();
        var restoredGraph = json.ToObject<GraphCoreSerialization>().FromSerialization();

        core.Be(restoredGraph);

        var restoredRegistry = new PrincipalRegistry(() => restoredGraph, NullLogger.Instance);
        var restoredPrincipals = restoredRegistry.GetAll().OrderBy(x => x.PrincipalId).ToArray();

        restoredPrincipals.Length.Be(principals.Length);
        restoredPrincipals[0].Be(principals[0]);
        restoredPrincipals[1].Be(principals[1]);

        foreach (var principal in principals)
        {
            var option = restoredRegistry.Get(principal.PrincipalId).BeOk();
            option.Value.Be(principal);

            var edges = restoredGraph.Edges.GetByFrom(principal.NodeKey, PrincipalIdentity.NodeReferenceType);
            edges.Count.Be(3);
        }
    }

}
