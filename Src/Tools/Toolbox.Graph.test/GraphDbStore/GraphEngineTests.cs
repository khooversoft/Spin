using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.GraphDbStore;

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

        var rec = new TestContractRecord("marko", 29);
        var recBase64 = rec.ToJson64();

        var addResult = await engine.ExecuteScalar($"add node key=node1, contract {{ '{recBase64}' }};", NullScopeContext.Instance);
        addResult.IsOk().Should().BeTrue();
        map.Nodes.Count.Should().Be(1);
        map.Edges.Count.Should().Be(0);
        ((InMemoryGraphFileStore)fileStore).Count.Should().Be(1);

        var readDataOption = await fileStore.Get("nodes/node1/node1___contract.json", NullScopeContext.Instance);
        readDataOption.IsOk().Should().BeTrue();

        TestContractRecord readRec = readDataOption.Return().ToObject<TestContractRecord>();
        readRec.NotNull();
        readRec.Name.Should().Be(rec.Name);
        readRec.Age.Should().Be(rec.Age);

        var selectResultOption = await engine.ExecuteScalar("select (key=node1) return contract;", NullScopeContext.Instance);
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

        var deleteResult = await engine.ExecuteScalar("delete (key=node1);", NullScopeContext.Instance);
        deleteResult.IsOk().Should().BeTrue();

        ((InMemoryGraphFileStore)fileStore).Count.Should().Be(0);
        map.Nodes.Count.Should().Be(0);
        map.Edges.Count.Should().Be(0);
        readDataOption = await fileStore.Get("nodes/node1/node1___contract.json", NullScopeContext.Instance);
        readDataOption.IsNotFound().Should().BeTrue();

        selectResultOption = await engine.ExecuteScalar("select (key=node1) return contract;", NullScopeContext.Instance);
        selectResultOption.IsOk().Should().BeTrue();
        selectResultOption.Return().Items.Length.Should().Be(0);
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

        var addResult = await engine.ExecuteScalar($"add node key=node1, lease {{ '{leaseBase64}' }}, contract {{ '{contractBase64}' }};", NullScopeContext.Instance);
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

        var selectResultOption = await engine.ExecuteScalar("select (key=node1) return contract;", NullScopeContext.Instance);
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

        var deleteResult = await engine.ExecuteScalar("delete (key=node1);", NullScopeContext.Instance);
        deleteResult.IsOk().Should().BeTrue();

        ((InMemoryGraphFileStore)fileStore).Count.Should().Be(0);
        map.Nodes.Count.Should().Be(0);
        map.Edges.Count.Should().Be(0);
        (await fileStore.Get("nodes/node1/node1___contract.json", NullScopeContext.Instance)).IsNotFound().Should().BeTrue();
        (await fileStore.Get("nodes/node1/node1___lease.json", NullScopeContext.Instance)).IsNotFound().Should().BeTrue();

        selectResultOption = await engine.ExecuteScalar("select (key=node1) return contract;", NullScopeContext.Instance);
        selectResultOption.IsOk().Should().BeTrue();
        selectResultOption.Return().Items.Length.Should().Be(0);
    }
}
