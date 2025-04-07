using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Graph.test.GraphDbStore;

/// <summary>
/// TODO: Add test case for updating multiple Nodes with the same data (lower priority)
/// </summary>
public class GraphEngineNodeDataTests
{
    private record TestContractRecord(string Name, int Age);
    private record TestLeaseRecord(string LeaseId, decimal Amount);

    [Fact]
    public async Task AddNodeWithData()
    {
        await using GraphHostService testClient = await GraphTestStartup.CreateGraphService();

        IFileStore fileStore = testClient.Services.GetRequiredService<IFileStore>();
        var context = testClient.CreateScopeContext<GraphEngineNodeDataTests>();

        // Add node with data
        var rec = new TestContractRecord("marko", 29);
        var recBase64 = rec.ToJson().ToBase64();

        var addResult = await testClient.Execute($"add node key=node1 set contract {{ '{recBase64}' }};", context);
        addResult.IsOk().Should().BeTrue();
        testClient.Map.Nodes.Count.Should().Be(1);
        testClient.Map.Edges.Count.Should().Be(0);
        await CheckFileStoreCount(fileStore, 1);

        // Verify data was writen correctly
        var readDataOption = await fileStore.File("nodes/node1/node1___contract.json").Get(context);
        readDataOption.IsOk().Should().BeTrue();
        TestContractRecord readRec = readDataOption.Return().ToObject<TestContractRecord>();
        readRec.NotNull();
        readRec.Name.Should().Be(rec.Name);
        readRec.Age.Should().Be(rec.Age);

        // Return data from graph and verify
        var selectResultOption = await testClient.Execute("select (key=node1) return contract;", context);
        selectResultOption.IsOk().Should().BeTrue();

        QueryResult selectResult = selectResultOption.Return();
        selectResult.Nodes.Count.Should().Be(1);
        selectResult.Edges.Count.Should().Be(0);
        selectResult.DataLinks.Count.Should().Be(1);

        selectResult.Nodes[0].Key.Should().Be("node1");

        selectResult.DataLinks[0].Action(x =>
        {
            x.NodeKey.Should().Be("node1");
            x.Name.Should().Be("contract");
            x.Data.NotNull();

            TestContractRecord readRec = x.Data.ToObject<TestContractRecord>();
            readRec.NotNull();
            readRec.Name.Should().Be(rec.Name);
            readRec.Age.Should().Be(rec.Age);
        });

        // Delete node and verify data was deleted as well (RI rules)
        var deleteResult = await testClient.Execute("delete (key=node1);", context);
        deleteResult.IsOk().Should().BeTrue();
        await CheckFileStoreCount(fileStore, 0);
        testClient.Map.Nodes.Count.Should().Be(0);
        testClient.Map.Edges.Count.Should().Be(0);
        readDataOption = await fileStore.File("nodes/node1/node1___contract.json").Get(context);
        readDataOption.IsNotFound().Should().BeTrue();

        selectResultOption = await testClient.Execute("select (key=node1) return contract;", context);
        selectResultOption.IsOk().Should().BeTrue();
        selectResultOption.Return().Action(x =>
        {
            x.Nodes.Count.Should().Be(0);
            x.Edges.Count.Should().Be(0);
            x.DataLinks.Count.Should().Be(0);
        });
    }

    [Fact]
    public async Task AddNodeWithDataAndDeleteData()
    {
        await using GraphHostService testClient = await GraphTestStartup.CreateGraphService();

        IFileStore fileStore = testClient.Services.GetRequiredService<IFileStore>();
        var context = testClient.CreateScopeContext<GraphEngineNodeDataTests>();

        // Add node with data
        var rec = new TestContractRecord("marko", 29);
        var recBase64 = rec.ToJson().ToBase64();

        var addResult = await testClient.Execute($"add node key=node1 set contract {{ '{recBase64}' }};", context);
        addResult.IsOk().Should().BeTrue();
        testClient.Map.Nodes.Count.Should().Be(1);
        testClient.Map.Edges.Count.Should().Be(0);
        await CheckFileStoreCount(fileStore, 1);

        // Verify data was writen correctly
        var readDataOption = await fileStore.File("nodes/node1/node1___contract.json").Get(context);
        readDataOption.IsOk().Should().BeTrue();
        TestContractRecord readRec = readDataOption.Return().ToObject<TestContractRecord>();
        readRec.NotNull();
        readRec.Name.Should().Be(rec.Name);
        readRec.Age.Should().Be(rec.Age);

        // Remove data from node and verify
        var removeData = await testClient.Execute("set node key=node1 set -contract;", context);
        removeData.IsOk().Should().BeTrue(removeData.ToString());
        await CheckFileStoreCount(fileStore, 0);
        testClient.Map.Nodes.Count.Should().Be(1);
        testClient.Map.Edges.Count.Should().Be(0);

        // Verify data has been deleted
        (await fileStore.File("nodes/node1/node1___contract.json").Get(context)).IsNotFound().Should().BeTrue();
        (await testClient.Execute("select (key=node1);", context)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Action(y =>
            {
                y.Nodes.Count.Should().Be(1);
                y.Nodes[0].DataMap.Count.Should().Be(0);
                y.Edges.Count.Should().Be(0);
                y.DataLinks.Count.Should().Be(0);
            });
        });

        // Verify data map has been updated
        var selectResultOption = await testClient.Execute("select (key=node1) return contract;", context);
        selectResultOption.IsOk().Should().BeTrue();
        selectResultOption.Return().Action(x =>
        {
            x.Nodes.Count.Should().Be(1);
            x.Nodes[0].DataMap.Count.Should().Be(0);
            x.Edges.Count.Should().Be(0);
            x.DataLinks.Count.Should().Be(0);
        });
    }

    [Fact]
    public async Task AddNodeWithTwoData()
    {
        await using GraphHostService testClient = await GraphTestStartup.CreateGraphService();

        IFileStore fileStore = testClient.Services.GetRequiredService<IFileStore>();
        var context = testClient.CreateScopeContext<GraphEngineNodeDataTests>();

        var contractRec = new TestContractRecord("marko", 29);
        var contractBase64 = contractRec.ToJson().ToBase64();
        var leaseRec = new TestLeaseRecord("lease#1", 100.0m);
        var leaseBase64 = leaseRec.ToJson().ToBase64();

        var addResult = await testClient.Execute($"add node key=node1 set lease {{ '{leaseBase64}' }}, contract {{ '{contractBase64}' }};", context);
        addResult.IsOk().Should().BeTrue();
        testClient.Map.Nodes.Count.Should().Be(1);
        testClient.Map.Edges.Count.Should().Be(0);
        await CheckFileStoreCount(fileStore, 2);

        (await fileStore.File("nodes/node1/node1___contract.json").Get(context)).Action((Action<Option<DataETag>>)(x =>
        {
            x.IsOk().Should().BeTrue();
            TestContractRecord readRec = x.Return().ToObject<TestContractRecord>();
            readRec.NotNull();
            readRec.Name.Should().Be(contractRec.Name);
            readRec.Age.Should().Be(contractRec.Age);
        }));

        (await fileStore.File("nodes/node1/node1___lease.json").Get(context)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            TestLeaseRecord readRec = x.Return().ToObject<TestLeaseRecord>();
            readRec.NotNull();
            readRec.LeaseId.Should().Be(leaseRec.LeaseId);
            readRec.Amount.Should().Be(leaseRec.Amount);
        });

        var selectResultOption = await testClient.Execute("select (key=node1) return contract;", context);
        selectResultOption.IsOk().Should().BeTrue();

        var selectResult = selectResultOption.Return();
        selectResult.Nodes.Count.Should().Be(1);
        selectResult.Nodes[0].Key.Should().Be("node1");
        selectResult.Edges.Count.Should().Be(0);
        selectResult.DataLinks.Count.Should().Be(1);
        selectResult.DataLinks.Get("contract").ThrowOnError().Return().Action(x =>
        {
            TestContractRecord readRec = x.Data.ToObject<TestContractRecord>();
            readRec.NotNull();
            readRec.Name.Should().Be(contractRec.Name);
            readRec.Age.Should().Be(contractRec.Age);
        });

        selectResultOption = await testClient.Execute("select (key=node1) return contract, lease;", context);
        selectResultOption.IsOk().Should().BeTrue();

        selectResult = selectResultOption.Return();
        selectResult.Nodes.Count.Should().Be(1);
        selectResult.Nodes[0].Key.Should().Be("node1");
        selectResult.Edges.Count.Should().Be(0);
        selectResult.DataLinks.Count.Should().Be(2);

        selectResult.DataLinks.Get("contract").ThrowOnError().Return().Action(x =>
        {
            TestContractRecord readRec = x.Data.ToObject<TestContractRecord>();
            readRec.NotNull();
            readRec.Name.Should().Be(contractRec.Name);
            readRec.Age.Should().Be(contractRec.Age);
        });

        selectResult.DataLinks.Get("lease").ThrowOnError().Return().Action(x =>
        {
            TestLeaseRecord readRec = x.Data.ToObject<TestLeaseRecord>();
            readRec.NotNull();
            readRec.LeaseId.Should().Be(leaseRec.LeaseId);
            readRec.Amount.Should().Be(leaseRec.Amount);
        });

        var deleteResult = await testClient.Execute("delete node key=node1;", context);
        deleteResult.IsOk().Should().BeTrue();

        await CheckFileStoreCount(fileStore, 0);
        testClient.Map.Nodes.Count.Should().Be(0);
        testClient.Map.Edges.Count.Should().Be(0);

        selectResultOption = await testClient.Execute("select (key=node1) return contract;", context);
        selectResultOption.IsOk().Should().BeTrue();

        selectResult = selectResultOption.Return();
        selectResult.Nodes.Count.Should().Be(0);
        selectResult.Edges.Count.Should().Be(0);
        selectResult.DataLinks.Count.Should().Be(0);
    }


    [Fact]
    public async Task AddNodeWithTwoDataDeletingOne()
    {
        await using GraphHostService testClient = await GraphTestStartup.CreateGraphService();

        IFileStore fileStore = testClient.Services.GetRequiredService<IFileStore>();
        var context = testClient.CreateScopeContext<GraphEngineNodeDataTests>();

        var contractRec = new TestContractRecord("marko", 29);
        var contractBase64 = contractRec.ToJson().ToBase64();
        var leaseRec = new TestLeaseRecord("lease#1", 100.0m);
        var leaseBase64 = leaseRec.ToJson().ToBase64();

        var addResult = await testClient.Execute($"add node key=node1 set lease {{ '{leaseBase64}' }}, contract {{ '{contractBase64}' }};", context);
        addResult.IsOk().Should().BeTrue();
        testClient.Map.Nodes.Count.Should().Be(1);
        testClient.Map.Edges.Count.Should().Be(0);
        await CheckFileStoreCount(fileStore, 2);

        (await fileStore.File("nodes/node1/node1___contract.json").Get(context)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            TestContractRecord readRec = x.Return().ToObject<TestContractRecord>();
            readRec.NotNull();
            readRec.Name.Should().Be(contractRec.Name);
            readRec.Age.Should().Be(contractRec.Age);
        });

        (await fileStore.File("nodes/node1/node1___lease.json").Get(context)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            TestLeaseRecord readRec = x.Return().ToObject<TestLeaseRecord>();
            readRec.NotNull();
            readRec.LeaseId.Should().Be(leaseRec.LeaseId);
            readRec.Amount.Should().Be(leaseRec.Amount);
        });

        // Delete data "contract" and verify data was deleted
        var removeDataOption = await testClient.Execute("set node key=node1 set -contract, t2=v2;", context);
        removeDataOption.IsOk().Should().BeTrue(removeDataOption.ToString());
        await CheckFileStoreCount(fileStore, 1);
        testClient.Map.Nodes.Count.Should().Be(1);
        testClient.Map.Nodes["node1"].Action(x =>
        {
            x.DataMap.Count.Should().Be(1);
            x.Tags.Count.Should().Be(1);
            x.Tags.TryGetValue("t2", out var value).Should().BeTrue();
            value.Should().Be("v2");
        });
        testClient.Map.Edges.Count.Should().Be(0);

        // Verify data has been deleted
        (await fileStore.File("nodes/node1/node1___contract.json").Get(context)).IsNotFound().Should().BeTrue();

        // Verify data should still exist
        (await fileStore.File("nodes/node1/node1___lease.json").Get(context)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            TestLeaseRecord readRec = x.Return().ToObject<TestLeaseRecord>();
            readRec.NotNull();
            readRec.LeaseId.Should().Be(leaseRec.LeaseId);
            readRec.Amount.Should().Be(leaseRec.Amount);
        });

        // Verify node
        (await testClient.Execute("select (key=node1);", context)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Action(y =>
            {
                y.Nodes.Count.Should().Be(1);
                y.Nodes[0].DataMap.Count.Should().Be(1);
                y.Nodes[0].DataMap.ContainsKey("lease").Should().BeTrue();
            });
        });

        var deleteResult = await testClient.Execute("delete node key=node1;", context);
        deleteResult.IsOk().Should().BeTrue();

        await CheckFileStoreCount(fileStore, 0);
        testClient.Map.Nodes.Count.Should().Be(0);
        testClient.Map.Edges.Count.Should().Be(0);

        var selectResultOption = await testClient.Execute("select (key=node1) return contract;", context);
        selectResultOption.IsOk().Should().BeTrue();
        selectResultOption.Return().Action(x =>
        {
            x.Nodes.Count.Should().Be(0);
            x.Edges.Count.Should().Be(0);
            x.DataLinks.Count.Should().Be(0);
        });
    }

    private async Task CheckFileStoreCount(IFileStore fileStore, int count)
    {
        (await fileStore.Search("nodes/**/*", NullScopeContext.Default)).Count.Should().Be(count);
    }
}
