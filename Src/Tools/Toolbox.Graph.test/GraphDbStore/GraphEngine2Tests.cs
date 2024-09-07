using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.GraphDbStore;

/// <summary>
/// TODO: Add test case for updating multiple Nodes with the same data (lower priority)
/// </summary>
public class GraphEngine2Tests
{
    private record TestContractRecord(string Name, int Age);
    private record TestLeaseRecord(string LeaseId, decimal Amount);

    [Fact]
    public async Task AddNodeWithData()
    {
        GraphTestClient2 engine = GraphTestStartup.CreateGraphTestHost2();
        GraphMap map = engine.ServiceProvider.GetRequiredService<GraphMap>();
        IGraphFileStore fileStore = engine.ServiceProvider.GetRequiredService<IGraphFileStore>();

        // Add node with data
        var rec = new TestContractRecord("marko", 29);
        var recBase64 = rec.ToJson64();

        var addResult = await engine.Execute($"add node key=node1 set contract {{ '{recBase64}' }};", NullScopeContext.Instance);
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

        QueryResult selectResult = selectResultOption.Return();
        selectResult.Nodes.Count.Should().Be(1);
        selectResult.Edges.Count.Should().Be(0);
        selectResult.Data.Count.Should().Be(1);

        selectResult.Nodes[0].Key.Should().Be("node1");

        selectResult.Data[0].Action(x =>
        {
            x.NodeKey.Should().Be("node1");
            x.Name.Should().Be("contract");
            x.Data.Should().NotBeNull();

            TestContractRecord readRec = x.Data.ToObject<TestContractRecord>();
            readRec.NotNull();
            readRec.Name.Should().Be(rec.Name);
            readRec.Age.Should().Be(rec.Age);
        });

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
        selectResultOption.Return().Action(x =>
        {
            x.Nodes.Count.Should().Be(0);
            x.Edges.Count.Should().Be(0);
            x.Data.Count.Should().Be(0);
        });
    }

    [Fact]
    public async Task AddNodeWithDataAndDeleteData()
    {
        GraphTestClient2 engine = GraphTestStartup.CreateGraphTestHost2();
        GraphMap map = engine.ServiceProvider.GetRequiredService<GraphMap>();
        IGraphFileStore fileStore = engine.ServiceProvider.GetRequiredService<IGraphFileStore>();

        // Add node with data
        var rec = new TestContractRecord("marko", 29);
        var recBase64 = rec.ToJson64();

        var addResult = await engine.Execute($"add node key=node1 set contract {{ '{recBase64}' }};", NullScopeContext.Instance);
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
        var removeData = await engine.Execute("update node key=node1 set -contract;", NullScopeContext.Instance);
        removeData.IsOk().Should().BeTrue(removeData.ToString());
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
                y.Nodes.Count.Should().Be(1);
                y.Nodes[0].DataMap.Count.Should().Be(0);
                y.Edges.Count.Should().Be(0);
                y.Data.Count.Should().Be(0);
            });
        });

        // Verify data map has been updated
        var selectResultOption = await engine.Execute("select (key=node1) return contract;", NullScopeContext.Instance);
        selectResultOption.IsOk().Should().BeTrue();
        selectResultOption.Return().Action(x =>
        {
            x.Nodes.Count.Should().Be(1);
            x.Nodes[0].DataMap.Count.Should().Be(0);
            x.Edges.Count.Should().Be(0);
            x.Data.Count.Should().Be(0);
        });
    }

    [Fact]
    public async Task AddNodeWithTwoData()
    {
        GraphTestClient2 engine = GraphTestStartup.CreateGraphTestHost2();
        GraphMap map = engine.ServiceProvider.GetRequiredService<GraphMap>();
        IGraphFileStore fileStore = engine.ServiceProvider.GetRequiredService<IGraphFileStore>();

        var contractRec = new TestContractRecord("marko", 29);
        var contractBase64 = contractRec.ToJson64();
        var leaseRec = new TestLeaseRecord("lease#1", 100.0m);
        var leaseBase64 = leaseRec.ToJson64();

        var addResult = await engine.Execute($"add node key=node1 set lease {{ '{leaseBase64}' }}, contract {{ '{contractBase64}' }};", NullScopeContext.Instance);
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

        var selectResult = selectResultOption.Return();
        selectResult.Nodes.Count.Should().Be(1);
        selectResult.Nodes[0].Key.Should().Be("node1");
        selectResult.Edges.Count.Should().Be(0);
        selectResult.Data.Count.Should().Be(1);
        selectResult.Data.Get("contract").ThrowOnError().Return().Action(x =>
        {
            TestContractRecord readRec = x.Data.ToObject<TestContractRecord>();
            readRec.NotNull();
            readRec.Name.Should().Be(contractRec.Name);
            readRec.Age.Should().Be(contractRec.Age);
        });

        selectResultOption = await engine.Execute("select (key=node1) return contract, lease;", NullScopeContext.Instance);
        selectResultOption.IsOk().Should().BeTrue();

        selectResult = selectResultOption.Return();
        selectResult.Nodes.Count.Should().Be(1);
        selectResult.Nodes[0].Key.Should().Be("node1");
        selectResult.Edges.Count.Should().Be(0);
        selectResult.Data.Count.Should().Be(2);

        selectResult.Data.Get("contract").ThrowOnError().Return().Action(x =>
        {
            TestContractRecord readRec = x.Data.ToObject<TestContractRecord>();
            readRec.NotNull();
            readRec.Name.Should().Be(contractRec.Name);
            readRec.Age.Should().Be(contractRec.Age);
        });

        selectResult.Data.Get("lease").ThrowOnError().Return().Action(x =>
        {
            TestLeaseRecord readRec = x.Data.ToObject<TestLeaseRecord>();
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

        selectResult = selectResultOption.Return();
        selectResult.Nodes.Count.Should().Be(0);
        selectResult.Edges.Count.Should().Be(0);
        selectResult.Data.Count.Should().Be(0);
    }


    [Fact]
    public async Task AddNodeWithTwoDataDeletingOne()
    {
        GraphTestClient2 engine = GraphTestStartup.CreateGraphTestHost2();
        GraphMap map = engine.ServiceProvider.GetRequiredService<GraphMap>();
        IGraphFileStore fileStore = engine.ServiceProvider.GetRequiredService<IGraphFileStore>();

        var contractRec = new TestContractRecord("marko", 29);
        var contractBase64 = contractRec.ToJson64();
        var leaseRec = new TestLeaseRecord("lease#1", 100.0m);
        var leaseBase64 = leaseRec.ToJson64();

        var addResult = await engine.Execute($"add node key=node1 set lease {{ '{leaseBase64}' }}, contract {{ '{contractBase64}' }};", NullScopeContext.Instance);
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
        var removeDataOption = await engine.Execute("update node key=node1 set -contract, t2=v2;", NullScopeContext.Instance);
        removeDataOption.IsOk().Should().BeTrue(removeDataOption.ToString());
        ((InMemoryGraphFileStore)fileStore).Count.Should().Be(1);
        map.Nodes.Count.Should().Be(1);
        map.Nodes["node1"].Action(x =>
        {
            x.DataMap.Count.Should().Be(1);
            x.Tags.Count.Should().Be(2);
            x.Tags.TryGetValue("t2", out var value).Should().BeTrue();
            value.Should().Be("v2");
        });
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
                y.Nodes.Count.Should().Be(1);
                y.Nodes[0].DataMap.Count.Should().Be(1);
                y.Nodes[0].DataMap.ContainsKey("lease").Should().BeTrue();
            });
        });

        var deleteResult = await engine.Execute("delete (key=node1);", NullScopeContext.Instance);
        deleteResult.IsOk().Should().BeTrue();

        ((InMemoryGraphFileStore)fileStore).Count.Should().Be(0);
        map.Nodes.Count.Should().Be(0);
        map.Edges.Count.Should().Be(0);

        var selectResultOption = await engine.Execute("select (key=node1) return contract;", NullScopeContext.Instance);
        selectResultOption.IsOk().Should().BeTrue();
        selectResultOption.Return().Action(x =>
        {
            x.Nodes.Count.Should().Be(0);
            x.Edges.Count.Should().Be(0);
            x.Data.Count.Should().Be(0);
        });
    }
}
