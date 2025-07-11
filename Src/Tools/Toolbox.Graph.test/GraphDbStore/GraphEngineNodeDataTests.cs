using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
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
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService();

        IFileStore fileStore = testClient.Services.GetRequiredService<IFileStore>();
        var context = testClient.CreateScopeContext<GraphEngineNodeDataTests>();

        // Add node with data
        var rec = new TestContractRecord("marko", 29);
        var recBase64 = rec.ToJson().ToBase64();

        var addResult = await testClient.Execute($"add node key=node1 set contract {{ '{recBase64}' }};", context);
        addResult.IsOk().BeTrue();
        testClient.Map.Nodes.Count.Be(1);
        testClient.Map.Edges.Count.Be(0);
        await CheckFileStoreCount(fileStore, 1);

        // Verify data was writen correctly
        var readDataOption = await fileStore.File("nodes/node1/node1___contract.json").Get(context);
        readDataOption.IsOk().BeTrue();
        TestContractRecord readRec = readDataOption.Return().ToObject<TestContractRecord>();
        readRec.NotNull();
        readRec.Name.Be(rec.Name);
        readRec.Age.Be(rec.Age);

        // Return data from graph and verify
        var selectResultOption = await testClient.Execute("select (key=node1) return contract;", context);
        selectResultOption.IsOk().BeTrue();

        QueryResult selectResult = selectResultOption.Return();
        selectResult.Nodes.Count.Be(1);
        selectResult.Edges.Count.Be(0);
        selectResult.DataLinks.Count.Be(1);

        selectResult.Nodes[0].Key.Be("node1");

        selectResult.DataLinks[0].Action(x =>
        {
            x.NodeKey.Be("node1");
            x.Name.Be("contract");
            x.Data.NotNull();

            TestContractRecord readRec = x.Data.ToObject<TestContractRecord>();
            readRec.NotNull();
            readRec.Name.Be(rec.Name);
            readRec.Age.Be(rec.Age);
        });

        // Delete node and verify data was deleted as well (RI rules)
        var deleteResult = await testClient.Execute("delete (key=node1);", context);
        deleteResult.IsOk().BeTrue();
        await CheckFileStoreCount(fileStore, 0);
        testClient.Map.Nodes.Count.Be(0);
        testClient.Map.Edges.Count.Be(0);
        readDataOption = await fileStore.File("nodes/node1/node1___contract.json").Get(context);
        readDataOption.IsNotFound().BeTrue();

        selectResultOption = await testClient.Execute("select (key=node1) return contract;", context);
        selectResultOption.IsOk().BeTrue();
        selectResultOption.Return().Action(x =>
        {
            x.Nodes.Count.Be(0);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);
        });
    }

    [Fact]
    public async Task AddNodeWithDataAndDeleteData()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService();

        IFileStore fileStore = testClient.Services.GetRequiredService<IFileStore>();
        var context = testClient.CreateScopeContext<GraphEngineNodeDataTests>();

        // Add node with data
        var rec = new TestContractRecord("marko", 29);
        var recBase64 = rec.ToJson().ToBase64();

        var addResult = await testClient.Execute($"add node key=node1 set contract {{ '{recBase64}' }};", context);
        addResult.IsOk().BeTrue();
        testClient.Map.Nodes.Count.Be(1);
        testClient.Map.Edges.Count.Be(0);
        await CheckFileStoreCount(fileStore, 1);

        // Verify data was writen correctly
        var readDataOption = await fileStore.File("nodes/node1/node1___contract.json").Get(context);
        readDataOption.IsOk().BeTrue();
        TestContractRecord readRec = readDataOption.Return().ToObject<TestContractRecord>();
        readRec.NotNull();
        readRec.Name.Be(rec.Name);
        readRec.Age.Be(rec.Age);

        // Remove data from node and verify
        var removeData = await testClient.Execute("set node key=node1 set -contract;", context);
        removeData.IsOk().BeTrue(removeData.ToString());
        await CheckFileStoreCount(fileStore, 0);
        testClient.Map.Nodes.Count.Be(1);
        testClient.Map.Edges.Count.Be(0);

        // Verify data has been deleted
        (await fileStore.File("nodes/node1/node1___contract.json").Get(context)).IsNotFound().BeTrue();
        (await testClient.Execute("select (key=node1);", context)).Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().Action(y =>
            {
                y.Nodes.Count.Be(1);
                y.Nodes[0].DataMap.Count.Be(0);
                y.Edges.Count.Be(0);
                y.DataLinks.Count.Be(0);
            });
        });

        // Verify data map has been updated
        var selectResultOption = await testClient.Execute("select (key=node1) return contract;", context);
        selectResultOption.IsOk().BeTrue();
        selectResultOption.Return().Action(x =>
        {
            x.Nodes.Count.Be(1);
            x.Nodes[0].DataMap.Count.Be(0);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);
        });
    }

    [Fact]
    public async Task AddNodeWithTwoData()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService();

        IFileStore fileStore = testClient.Services.GetRequiredService<IFileStore>();
        var context = testClient.CreateScopeContext<GraphEngineNodeDataTests>();

        var contractRec = new TestContractRecord("marko", 29);
        var contractBase64 = contractRec.ToJson().ToBase64();
        var leaseRec = new TestLeaseRecord("lease#1", 100.0m);
        var leaseBase64 = leaseRec.ToJson().ToBase64();

        var addResult = await testClient.Execute($"add node key=node1 set lease {{ '{leaseBase64}' }}, contract {{ '{contractBase64}' }};", context);
        addResult.IsOk().BeTrue();
        testClient.Map.Nodes.Count.Be(1);
        testClient.Map.Edges.Count.Be(0);
        await CheckFileStoreCount(fileStore, 2);

        (await fileStore.File("nodes/node1/node1___contract.json").Get(context)).Action((Action<Option<DataETag>>)(x =>
        {
            x.IsOk().BeTrue();
            TestContractRecord readRec = x.Return().ToObject<TestContractRecord>();
            readRec.NotNull();
            readRec.Name.Be(contractRec.Name);
            readRec.Age.Be(contractRec.Age);
        }));

        (await fileStore.File("nodes/node1/node1___lease.json").Get(context)).Action(x =>
        {
            x.IsOk().BeTrue();
            TestLeaseRecord readRec = x.Return().ToObject<TestLeaseRecord>();
            readRec.NotNull();
            readRec.LeaseId.Be(leaseRec.LeaseId);
            readRec.Amount.Be(leaseRec.Amount);
        });

        var selectResultOption = await testClient.Execute("select (key=node1) return contract;", context);
        selectResultOption.IsOk().BeTrue();

        var selectResult = selectResultOption.Return();
        selectResult.Nodes.Count.Be(1);
        selectResult.Nodes[0].Key.Be("node1");
        selectResult.Edges.Count.Be(0);
        selectResult.DataLinks.Count.Be(1);
        selectResult.DataLinks.Get("contract").ThrowOnError().Return().Action(x =>
        {
            TestContractRecord readRec = x.Data.ToObject<TestContractRecord>();
            readRec.NotNull();
            readRec.Name.Be(contractRec.Name);
            readRec.Age.Be(contractRec.Age);
        });

        selectResultOption = await testClient.Execute("select (key=node1) return contract, lease;", context);
        selectResultOption.IsOk().BeTrue();

        selectResult = selectResultOption.Return();
        selectResult.Nodes.Count.Be(1);
        selectResult.Nodes[0].Key.Be("node1");
        selectResult.Edges.Count.Be(0);
        selectResult.DataLinks.Count.Be(2);

        selectResult.DataLinks.Get("contract").ThrowOnError().Return().Action(x =>
        {
            TestContractRecord readRec = x.Data.ToObject<TestContractRecord>();
            readRec.NotNull();
            readRec.Name.Be(contractRec.Name);
            readRec.Age.Be(contractRec.Age);
        });

        selectResult.DataLinks.Get("lease").ThrowOnError().Return().Action(x =>
        {
            TestLeaseRecord readRec = x.Data.ToObject<TestLeaseRecord>();
            readRec.NotNull();
            readRec.LeaseId.Be(leaseRec.LeaseId);
            readRec.Amount.Be(leaseRec.Amount);
        });

        var deleteResult = await testClient.Execute("delete node key=node1;", context);
        deleteResult.IsOk().BeTrue();

        await CheckFileStoreCount(fileStore, 0);
        testClient.Map.Nodes.Count.Be(0);
        testClient.Map.Edges.Count.Be(0);

        selectResultOption = await testClient.Execute("select (key=node1) return contract;", context);
        selectResultOption.IsOk().BeTrue();

        selectResult = selectResultOption.Return();
        selectResult.Nodes.Count.Be(0);
        selectResult.Edges.Count.Be(0);
        selectResult.DataLinks.Count.Be(0);
    }


    [Fact]
    public async Task AddNodeWithTwoDataDeletingOne()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService();

        IFileStore fileStore = testClient.Services.GetRequiredService<IFileStore>();
        var context = testClient.CreateScopeContext<GraphEngineNodeDataTests>();

        var contractRec = new TestContractRecord("marko", 29);
        var contractBase64 = contractRec.ToJson().ToBase64();
        var leaseRec = new TestLeaseRecord("lease#1", 100.0m);
        var leaseBase64 = leaseRec.ToJson().ToBase64();

        var addResult = await testClient.Execute($"add node key=node1 set lease {{ '{leaseBase64}' }}, contract {{ '{contractBase64}' }};", context);
        addResult.IsOk().BeTrue();
        testClient.Map.Nodes.Count.Be(1);
        testClient.Map.Edges.Count.Be(0);
        await CheckFileStoreCount(fileStore, 2);

        (await fileStore.File("nodes/node1/node1___contract.json").Get(context)).Action(x =>
        {
            x.IsOk().BeTrue();
            TestContractRecord readRec = x.Return().ToObject<TestContractRecord>();
            readRec.NotNull();
            readRec.Name.Be(contractRec.Name);
            readRec.Age.Be(contractRec.Age);
        });

        (await fileStore.File("nodes/node1/node1___lease.json").Get(context)).Action(x =>
        {
            x.IsOk().BeTrue();
            TestLeaseRecord readRec = x.Return().ToObject<TestLeaseRecord>();
            readRec.NotNull();
            readRec.LeaseId.Be(leaseRec.LeaseId);
            readRec.Amount.Be(leaseRec.Amount);
        });

        // Delete data "contract" and verify data was deleted
        var removeDataOption = await testClient.Execute("set node key=node1 set -contract, t2=v2;", context);
        removeDataOption.IsOk().BeTrue(removeDataOption.ToString());
        await CheckFileStoreCount(fileStore, 1);
        testClient.Map.Nodes.Count.Be(1);
        testClient.Map.Nodes["node1"].Action(x =>
        {
            x.DataMap.Count.Be(1);
            x.Tags.Count.Be(1);
            x.Tags.TryGetValue("t2", out var value).BeTrue();
            value.Be("v2");
        });
        testClient.Map.Edges.Count.Be(0);

        // Verify data has been deleted
        (await fileStore.File("nodes/node1/node1___contract.json").Get(context)).IsNotFound().BeTrue();

        // Verify data should still exist
        (await fileStore.File("nodes/node1/node1___lease.json").Get(context)).Action(x =>
        {
            x.IsOk().BeTrue();
            TestLeaseRecord readRec = x.Return().ToObject<TestLeaseRecord>();
            readRec.NotNull();
            readRec.LeaseId.Be(leaseRec.LeaseId);
            readRec.Amount.Be(leaseRec.Amount);
        });

        // Verify node
        (await testClient.Execute("select (key=node1);", context)).Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().Action(y =>
            {
                y.Nodes.Count.Be(1);
                y.Nodes[0].DataMap.Count.Be(1);
                y.Nodes[0].DataMap.ContainsKey("lease").BeTrue();
            });
        });

        var deleteResult = await testClient.Execute("delete node key=node1;", context);
        deleteResult.IsOk().BeTrue();

        await CheckFileStoreCount(fileStore, 0);
        testClient.Map.Nodes.Count.Be(0);
        testClient.Map.Edges.Count.Be(0);

        var selectResultOption = await testClient.Execute("select (key=node1) return contract;", context);
        selectResultOption.IsOk().BeTrue();
        selectResultOption.Return().Action(x =>
        {
            x.Nodes.Count.Be(0);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);
        });
    }

    private async Task CheckFileStoreCount(IFileStore fileStore, int count)
    {
        (await fileStore.Search("nodes/**/*", NullScopeContext.Instance)).Count.Be(count);
    }
}
