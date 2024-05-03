using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Context;

public class ChangeTrxTests
{
    [Fact]
    public void NodeSerialization()
    {
        DateTime dt = DateTime.UtcNow;
        Guid trxId = Guid.NewGuid();
        Guid logKey = Guid.NewGuid();
        GraphNode currentNode1 = new GraphNode("node1", "tag1=v1".ToTags(), dt, ["link1", "link2"], GraphDataLinkTool.Empty);
        GraphNode currentNode2 = new GraphNode("node1", "tag1=v1".ToTags(), dt, ["link1", "link2"], GraphDataLinkTool.Empty);

        (currentNode1 == currentNode2).Should().BeTrue();

        var source = new ChangeTrx(ChangeTrxType.NodeAdd, trxId, logKey, currentNode1, null, dt);
        var compareTo = new ChangeTrx(ChangeTrxType.NodeAdd, trxId, logKey, currentNode2, null, dt);

        (source == compareTo).Should().BeTrue();

        var data = source.ToJson();
        ChangeTrx readTrx = data.ToObject<ChangeTrx>().NotNull();
        (source == readTrx).Should().BeTrue();
    }

    [Fact]
    public void NodeSerializationWithNewValue()
    {
        DateTime dt = DateTime.UtcNow;
        Guid trxId = Guid.NewGuid();
        Guid logKey = Guid.NewGuid();
        GraphNode currentNode1 = new GraphNode("node1", "tag1=v1".ToTags(), dt, ["link1", "link2"], GraphDataLinkTool.Empty);
        GraphNode currentNode2 = new GraphNode("node1", "tag1=v1".ToTags(), dt, ["link1", "link2"], GraphDataLinkTool.Empty);
        GraphNode newValueNode1 = new GraphNode("node2", "tag1=v1".ToTags(), dt, ["link4", "link5"], GraphDataLinkTool.Empty);
        GraphNode newValueNode2 = new GraphNode("node2", "tag1=v1".ToTags(), dt, ["link4", "link5"], GraphDataLinkTool.Empty);

        (currentNode1 == currentNode2).Should().BeTrue();

        var source = new ChangeTrx(ChangeTrxType.NodeAdd, trxId, logKey, currentNode1, newValueNode1, dt);
        var compareTo = new ChangeTrx(ChangeTrxType.NodeAdd, trxId, logKey, currentNode2, newValueNode2, dt);

        (source == compareTo).Should().BeTrue();

        var data = source.ToJson();
        ChangeTrx readTrx = data.ToObject<ChangeTrx>().NotNull();
        (source == readTrx).Should().BeTrue();
    }

    [Fact]
    public void EdgeSerialization()
    {
        DateTime dt = DateTime.UtcNow;
        Guid key = Guid.NewGuid();
        Guid trxId = Guid.NewGuid();
        Guid logKey = Guid.NewGuid();
        const string fromKey = "fromKey1";
        const string toKey = "toKey1";
        const string edgeType = "edgeType";

        GraphEdge edge1 = new GraphEdge(key, fromKey, toKey, edgeType, "tag1=v1".ToTags(), dt);
        GraphEdge edge2 = new GraphEdge(key, fromKey, toKey, edgeType, "tag1=v1".ToTags(), dt);

        (edge1 == edge2).Should().BeTrue();

        var source = new ChangeTrx(ChangeTrxType.EdgeAdd, trxId, logKey, edge1, null, dt);
        var compareTo = new ChangeTrx(ChangeTrxType.EdgeAdd, trxId, logKey, edge2, null, dt);

        (source == compareTo).Should().BeTrue();

        var data = source.ToJson();
        ChangeTrx readTrx = data.ToObject<ChangeTrx>().NotNull();
        (source == readTrx).Should().BeTrue();
    }

    [Fact]
    public void EdgeSerializationWithNewValue()
    {
        DateTime dt = DateTime.UtcNow;
        Guid key = Guid.NewGuid();
        Guid trxId = Guid.NewGuid();
        Guid logKey = Guid.NewGuid();
        const string fromKey1 = "fromKey1";
        const string toKey1 = "toKey1";
        const string edgeType1 = "edgeType1";
        const string fromKey2 = "fromKey2";
        const string toKey2 = "toKey2";
        const string edgeType2 = "edgeType2";

        GraphEdge currentEdge1 = new GraphEdge(key, fromKey1, toKey1, edgeType1, "tag1=v1".ToTags(), dt);
        GraphEdge currentEdge2 = new GraphEdge(key, fromKey1, toKey1, edgeType1, "tag1=v1".ToTags(), dt);
        GraphEdge newEdge1 = new GraphEdge(key, fromKey2, toKey2, edgeType2, "tag1=v1".ToTags(), dt);
        GraphEdge newEdge2 = new GraphEdge(key, fromKey2, toKey2, edgeType2, "tag1=v1".ToTags(), dt);

        (currentEdge1 == currentEdge2).Should().BeTrue();

        var source = new ChangeTrx(ChangeTrxType.EdgeAdd, trxId, logKey, currentEdge1, newEdge1, dt);
        var compareTo = new ChangeTrx(ChangeTrxType.EdgeAdd, trxId, logKey, currentEdge2, newEdge2, dt);

        (source == compareTo).Should().BeTrue();
    }
}
