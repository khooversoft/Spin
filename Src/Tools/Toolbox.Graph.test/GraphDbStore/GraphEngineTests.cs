using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.GraphDbStore;

/// <summary>
/// TODO: Add test case for updating multiple Nodes with the same data (lower priority)
/// </summary>
public class GraphEngineTests
{
    private record TestContractRecord(string Name, int Age);
    private record TestLeaseRecord(string LeaseId, decimal Amount);

    [Fact]
    public async Task AddNodeWithData()
    {
        GraphTestClient engine = GraphTestStartup.CreateGraphTestHost();
        GraphMap map = engine.ServiceProvider.GetRequiredService<GraphMap>();
        IGraphFileStore fileStore = engine.ServiceProvider.GetRequiredService<IGraphFileStore>();

        // Add node with data
        var rec = new TestContractRecord("marko", 29);
        var recBase64 = rec.ToJson64();

        var addResult = await engine.Execute($"add node key=node1, contract {{ '{recBase64}' }};", NullScopeContext.Instance);
        addResult.IsOk().Should().BeTrue();
        map.Nodes.Count.Should().Be(1);
        map.Edges.Count.Should().Be(0);
        ((InMemoryGraphFileStore)fileStore).Count.Should().Be(1);

        // Verify data was writen correctly
        var readDataOption = await fileStore.Get("nodes/node1/node1___contract.json", NullScopeContext.Instance);
        readDataOption.IsOk().Should().BeTrue();
        TestContractRecord readRec = readDataOption.Return().ToObject<TestContractRecord>();
        readRec.NotNull();
        readRec.Name.Should().Be(rec.Name);
        readRec.Age.Should().Be(rec.Age);

        // Return data from graph and verify
        var selectResultOption = await engine.Execute("select (key=node1) return contract;", NullScopeContext.Instance);
        selectResultOption.IsOk().Should().BeTrue();

        GraphQueryResult selectResult = selectResultOption.Return();
        selectResult.Items.Length.Should().Be(1);
        var node = selectResult.Items.OfType<GraphNode>().First();
        node.Key.Should().Be("node1");

        selectResult.ReturnNames.Count.Should().Be(1);
        selectResult.ReturnNames.First().Action((Action<KeyValuePair<string, DataETag>>)(x =>
        {
            x.Key.Should().Be("contract");
            x.Value.Should().NotBeNull();

            TestContractRecord readRec = x.Value.ToObject<TestContractRecord>();
            readRec.NotNull();
            readRec.Name.Should().Be(rec.Name);
            readRec.Age.Should().Be(rec.Age);
        }));

        // Delete node and verify data was deleted as well (RI rules)
        var deleteResult = await engine.Execute("delete (key=node1);", NullScopeContext.Instance);
        deleteResult.IsOk().Should().BeTrue();
        ((InMemoryGraphFileStore)fileStore).Count.Should().Be(0);
        map.Nodes.Count.Should().Be(0);
        map.Edges.Count.Should().Be(0);
        readDataOption = await fileStore.Get("nodes/node1/node1___contract.json", NullScopeContext.Instance);
        readDataOption.IsNotFound().Should().BeTrue();

        selectResultOption = await engine.Execute("select (key=node1) return contract;", NullScopeContext.Instance);
        selectResultOption.IsOk().Should().BeTrue();
        selectResultOption.Return().Items.Length.Should().Be(0);
    }


    [Fact]
    public async Task AddNodeWithDataAndDeleteData()
    {
        GraphTestClient engine = GraphTestStartup.CreateGraphTestHost();
        GraphMap map = engine.ServiceProvider.GetRequiredService<GraphMap>();
        IGraphFileStore fileStore = engine.ServiceProvider.GetRequiredService<IGraphFileStore>();

        // Add node with data
        var rec = new TestContractRecord("marko", 29);
        var recBase64 = rec.ToJson64();

        var addResult = await engine.Execute($"add node key=node1, contract {{ '{recBase64}' }};", NullScopeContext.Instance);
        addResult.IsOk().Should().BeTrue();
        map.Nodes.Count.Should().Be(1);
        map.Edges.Count.Should().Be(0);
        ((InMemoryGraphFileStore)fileStore).Count.Should().Be(1);

        // Verify data was writen correctly
        var readDataOption = await fileStore.Get("nodes/node1/node1___contract.json", NullScopeContext.Instance);
        readDataOption.IsOk().Should().BeTrue();
        TestContractRecord readRec = readDataOption.Return().ToObject<TestContractRecord>();
        readRec.NotNull();
        readRec.Name.Should().Be(rec.Name);
        readRec.Age.Should().Be(rec.Age);

        // Remove data from node and verify
        var removeData = await engine.Execute("update (key=node1) set -contract;", NullScopeContext.Instance);
        removeData.IsOk().Should().BeTrue();
        ((InMemoryGraphFileStore)fileStore).Count.Should().Be(0);
        map.Nodes.Count.Should().Be(1);
        map.Edges.Count.Should().Be(0);

        // Verify data has been deleted
        (await fileStore.Get("nodes/node1/node1___contract.json", NullScopeContext.Instance)).IsNotFound().Should().BeTrue();
        (await engine.Execute("select (key=node1);", NullScopeContext.Instance)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Action(y =>
            {
                y.Items.Length.Should().Be(1);
                var n = y.Items.OfType<GraphNode>().First();
                n.DataMap.Count.Should().Be(0);
            });
        });

        // Verify data map has been updated
        var selectResultOption = await engine.Execute("select (key=node1) return contract;", NullScopeContext.Instance);
        selectResultOption.IsOk().Should().BeTrue();
        selectResultOption.Return().Action(x =>
        {
            x.Items.Length.Should().Be(1);
            var nodes = x.Items.OfType<GraphNode>().ToArray();
            nodes.Length.Should().Be(1);
            nodes[0].DataMap.Count.Should().Be(0);
            x.ReturnNames.Count.Should().Be(0);
        });
    }

    [Fact]
    public async Task AddNodeWithTwoData()
    {
        GraphTestClient engine = GraphTestStartup.CreateGraphTestHost();
        GraphMap map = engine.ServiceProvider.GetRequiredService<GraphMap>();
        IGraphFileStore fileStore = engine.ServiceProvider.GetRequiredService<IGraphFileStore>();

        var contractRec = new TestContractRecord("marko", 29);
        var contractBase64 = contractRec.ToJson64();
        var leaseRec = new TestLeaseRecord("lease#1", 100.0m);
        var leaseBase64 = leaseRec.ToJson64();

        var addResult = await engine.Execute($"add node key=node1, lease {{ '{leaseBase64}' }}, contract {{ '{contractBase64}' }};", NullScopeContext.Instance);
        addResult.IsOk().Should().BeTrue();
        map.Nodes.Count.Should().Be(1);
        map.Edges.Count.Should().Be(0);
        ((InMemoryGraphFileStore)fileStore).Count.Should().Be(2);

        (await fileStore.Get("nodes/node1/node1___contract.json", NullScopeContext.Instance)).Action((Action<Option<DataETag>>)(x =>
        {
            x.IsOk().Should().BeTrue();
            TestContractRecord readRec = x.Return().ToObject<TestContractRecord>();
            readRec.NotNull();
            readRec.Name.Should().Be(contractRec.Name);
            readRec.Age.Should().Be(contractRec.Age);
        }));

        (await fileStore.Get("nodes/node1/node1___lease.json", NullScopeContext.Instance)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            TestLeaseRecord readRec = x.Return().ToObject<TestLeaseRecord>();
            readRec.NotNull();
            readRec.LeaseId.Should().Be(leaseRec.LeaseId);
            readRec.Amount.Should().Be(leaseRec.Amount);
        });

        var selectResultOption = await engine.Execute("select (key=node1) return contract;", NullScopeContext.Instance);
        selectResultOption.IsOk().Should().BeTrue();

        GraphQueryResult selectResult = selectResultOption.Return();
        selectResult.Items.Length.Should().Be(1);
        var node = selectResult.Items.OfType<GraphNode>().First();
        node.Key.Should().Be("node1");

        selectResult.ReturnNames.Count.Should().Be(2);
        selectResult.ReturnNames["contract"].Action(x =>
        {
            TestContractRecord readRec = x.ToObject<TestContractRecord>();
            readRec.NotNull();
            readRec.Name.Should().Be(contractRec.Name);
            readRec.Age.Should().Be(contractRec.Age);
        });
        selectResult.ReturnNames["lease"].Action(x =>
        {
            TestLeaseRecord readRec = x.ToObject<TestLeaseRecord>();
            readRec.NotNull();
            readRec.LeaseId.Should().Be(leaseRec.LeaseId);
            readRec.Amount.Should().Be(leaseRec.Amount);
        });

        var deleteResult = await engine.Execute("delete (key=node1);", NullScopeContext.Instance);
        deleteResult.IsOk().Should().BeTrue();

        ((InMemoryGraphFileStore)fileStore).Count.Should().Be(0);
        map.Nodes.Count.Should().Be(0);
        map.Edges.Count.Should().Be(0);

        selectResultOption = await engine.Execute("select (key=node1) return contract;", NullScopeContext.Instance);
        selectResultOption.IsOk().Should().BeTrue();
        selectResultOption.Return().Items.Length.Should().Be(0);
    }


    [Fact]
    public async Task AddNodeWithTwoDataDeletingOne()
    {
        GraphTestClient engine = GraphTestStartup.CreateGraphTestHost();
        GraphMap map = engine.ServiceProvider.GetRequiredService<GraphMap>();
        IGraphFileStore fileStore = engine.ServiceProvider.GetRequiredService<IGraphFileStore>();

        var contractRec = new TestContractRecord("marko", 29);
        var contractBase64 = contractRec.ToJson64();
        var leaseRec = new TestLeaseRecord("lease#1", 100.0m);
        var leaseBase64 = leaseRec.ToJson64();

        var addResult = await engine.Execute($"add node key=node1, lease {{ '{leaseBase64}' }}, contract {{ '{contractBase64}' }};", NullScopeContext.Instance);
        addResult.IsOk().Should().BeTrue();
        map.Nodes.Count.Should().Be(1);
        map.Edges.Count.Should().Be(0);
        ((InMemoryGraphFileStore)fileStore).Count.Should().Be(2);

        (await fileStore.Get("nodes/node1/node1___contract.json", NullScopeContext.Instance)).Action((Action<Option<DataETag>>)(x =>
        {
            x.IsOk().Should().BeTrue();
            TestContractRecord readRec = x.Return().ToObject<TestContractRecord>();
            readRec.NotNull();
            readRec.Name.Should().Be(contractRec.Name);
            readRec.Age.Should().Be(contractRec.Age);
        }));

        (await fileStore.Get("nodes/node1/node1___lease.json", NullScopeContext.Instance)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            TestLeaseRecord readRec = x.Return().ToObject<TestLeaseRecord>();
            readRec.NotNull();
            readRec.LeaseId.Should().Be(leaseRec.LeaseId);
            readRec.Amount.Should().Be(leaseRec.Amount);
        });

        // Delete data "contract" and verify data was deleted
        var removeData = await engine.Execute("update (key=node1) set -contract;", NullScopeContext.Instance);
        removeData.IsOk().Should().BeTrue();
        ((InMemoryGraphFileStore)fileStore).Count.Should().Be(1);
        map.Nodes.Count.Should().Be(1);
        map.Edges.Count.Should().Be(0);

        // Verify data has been deleted
        (await fileStore.Get("nodes/node1/node1___contract.json", NullScopeContext.Instance)).IsNotFound().Should().BeTrue();

        // Verify data should still exist
        (await fileStore.Get("nodes/node1/node1___lease.json", NullScopeContext.Instance)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            TestLeaseRecord readRec = x.Return().ToObject<TestLeaseRecord>();
            readRec.NotNull();
            readRec.LeaseId.Should().Be(leaseRec.LeaseId);
            readRec.Amount.Should().Be(leaseRec.Amount);
        });

        // Verify node
        (await engine.Execute("select (key=node1);", NullScopeContext.Instance)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Action(y =>
            {
                y.Items.Length.Should().Be(1);
                var n = y.Items.OfType<GraphNode>().First();
                n.DataMap.Count.Should().Be(1);
                n.DataMap.ContainsKey("lease").Should().BeTrue();
            });
        });

        var deleteResult = await engine.Execute("delete (key=node1);", NullScopeContext.Instance);
        deleteResult.IsOk().Should().BeTrue();

        ((InMemoryGraphFileStore)fileStore).Count.Should().Be(0);
        map.Nodes.Count.Should().Be(0);
        map.Edges.Count.Should().Be(0);

        var selectResultOption = await engine.Execute("select (key=node1) return contract;", NullScopeContext.Instance);
        selectResultOption.IsOk().Should().BeTrue();
        selectResultOption.Return().Items.Length.Should().Be(0);
    }
}
