//using FluentAssertions;
//using SpinCluster.sdk.Actors.Directory;
//using Toolbox.Extensions;

//namespace SpinClusterApi.test.Directory;

//public class DirectoryBatchSerializationTests
//{
//    [Fact]
//    public void SimpleBatch()
//    {
//        var list = new IDirectoryGraph[]
//        {
//            new DirectoryNode { Key = "Node1" },
//            new DirectoryNode { Key = "Node2" },
//            new RemoveNode { NodeKey = "Node3" },
//            new RemoveEdge { EdgeKey = Guid.NewGuid() },
//            new DirectoryEdge { FromKey = "Node1", ToKey = "Node2" },
//            new RemoveEdge { EdgeKey = Guid.NewGuid() },
//        };

//        var batch = new DirectoryBatch
//        {
//            Items = list.ToArray(),
//        };

//        string json = batch.ToJson();

//        var readBatch = json.ToObject<DirectoryBatch>();
//        readBatch.Should().NotBeNull();
//        readBatch!.Items.Count.Should().Be(list.Length);
//        Enumerable.SequenceEqual(readBatch.Items, list).Should().BeTrue();

//        readBatch.Items.OfType<DirectoryNode>().ToArray().Action(x =>
//        {
//            x.Length.Should().Be(2);
//            Enumerable.SequenceEqual(x, list.Take(2)).Should().BeTrue();
//        });

//        readBatch.Items.OfType<DirectoryEdge>().ToArray().Action(x =>
//        {
//            x.Length.Should().Be(1);
//            Enumerable.SequenceEqual(x, list.OfType<DirectoryEdge>()).Should().BeTrue();
//        });

//        readBatch.Items.OfType<RemoveNode>().ToArray().Action(x =>
//        {
//            x.Length.Should().Be(1);
//            Enumerable.SequenceEqual(x, list.OfType<RemoveNode>()).Should().BeTrue();
//        });

//        readBatch.Items.OfType<RemoveEdge>().ToArray().Action(x =>
//        {
//            x.Length.Should().Be(2);
//            Enumerable.SequenceEqual(x, list.OfType<RemoveEdge>()).Should().BeTrue();
//        });
//    }
//}
