//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Toolbox.Azure;
//using Toolbox.Extensions;
//using Toolbox.Graph.test.Application;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;
//using Xunit.Abstractions;

//namespace Toolbox.Graph.test.GraphDbStore;

///// <summary>
///// TODO: Add test case for updating multiple Nodes with the same data (lower priority)
///// </summary>
//public class GraphEngineNodeDataTests
//{
//    private record TestContractRecord(string Name, int Age);
//    private record TestLeaseRecord(string LeaseId, decimal Amount);

//    private readonly ITestOutputHelper _logOutput;
//    public GraphEngineNodeDataTests(ITestOutputHelper logOutput) => _logOutput = logOutput;

//    private async Task<IHost> CreateService(bool useDataLake)
//    {
//        const string basePath = "basePath";

//        DatalakeOption datalakeOption = TestApplication.ReadDatalakeOption("test-GraphEngineNodeDataTests");

//        var host = Host.CreateDefaultBuilder()
//            .AddDebugLogging(x => _logOutput.WriteLine(x))
//            .ConfigureServices((context, services) =>
//            {
//                if (useDataLake)
//                    services.AddDatalakeFileStore(datalakeOption);
//                else
//                    services.AddInMemoryKeyStore();

//                services.AddGraphEngine(config => config.BasePath = basePath);
//            })
//            .Build();

//        var context = host.Services.GetRequiredService<ILogger<GraphDbRoundTripTests>>();

//        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();
//        await keyStore.DeleteFolder(basePath);
//        (await keyStore.Search($"{basePath}/***")).Count().Be(0);

//        IGraphEngine graphEngine = host.Services.GetRequiredService<IGraphEngine>();
//        await graphEngine.DataManager.LoadDatabase();

//        return host;
//    }

//    [Theory]
//    [InlineData(false)]
//    [InlineData(true)]
//    public async Task AddNodeWithData(bool useDataLake)
//    {
//        using var host = await CreateService(useDataLake);
//        var graphClient = host.Services.GetRequiredService<IGraphClient>();
//        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();
//        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

//        // Add node with data
//        var rec = new TestContractRecord("marko", 29);
//        var recBase64 = rec.ToJson().ToBase64();

//        var addResult = await graphClient.Execute($"add node key=node1 set contract {{ '{recBase64}' }};");
//        addResult.BeOk();
//        graphEngine.DataManager.GetMap().Nodes.Count.Be(1);
//        graphEngine.DataManager.GetMap().Edges.Count.Be(0);
//        await CheckFileStoreCount(keyStore, 1);

//        string dataFilePath = "basepath/data/d1/da/nodes/node1/node1___contract.json";

//        // Verify data was written correctly
//        var readDataOption = (await keyStore.Get(dataFilePath)).BeOk();
//        TestContractRecord readRec = readDataOption.Return().ToObject<TestContractRecord>();
//        readRec.NotNull();
//        readRec.Name.Be(rec.Name);
//        readRec.Age.Be(rec.Age);

//        // Return data from graph and verify
//        var selectResultOption = (await graphClient.Execute("select (key=node1) return contract;")).BeOk();

//        QueryResult selectResult = selectResultOption.Return();
//        selectResult.Nodes.Count.Be(1);
//        selectResult.Edges.Count.Be(0);
//        selectResult.DataLinks.Count.Be(1);

//        selectResult.Nodes[0].Key.Be("node1");

//        selectResult.DataLinks[0].Action(x =>
//        {
//            x.NodeKey.Be("node1");
//            x.Name.Be("contract");
//            x.Data.NotNull();

//            TestContractRecord readRec = x.Data.ToObject<TestContractRecord>();
//            readRec.NotNull();
//            readRec.Name.Be(rec.Name);
//            readRec.Age.Be(rec.Age);
//        });

//        // Delete node and verify data was deleted as well (RI rules)
//        var deleteResult = await graphClient.Execute("delete (key=node1);");
//        deleteResult.BeOk();
//        await CheckFileStoreCount(keyStore, 0);
//        graphEngine.DataManager.GetMap().Nodes.Count.Be(0);
//        graphEngine.DataManager.GetMap().Edges.Count.Be(0);
//        readDataOption = (await keyStore.Get(dataFilePath)).BeNotFound();

//        selectResultOption = (await graphClient.Execute("select (key=node1) return contract;")).BeOk();
//        selectResultOption.Return().Action(x =>
//        {
//            x.Nodes.Count.Be(0);
//            x.Edges.Count.Be(0);
//            x.DataLinks.Count.Be(0);
//        });
//    }

//    [Theory]
//    [InlineData(false)]
//    [InlineData(true)]
//    public async Task AddNodeWithDataAndDeleteData(bool useDataLake)
//    {
//        using var host = await CreateService(useDataLake);
//        var graphClient = host.Services.GetRequiredService<IGraphClient>();
//        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();
//        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();
//        KeySpace keySpace = keyStore as KeySpace ?? throw new ArgumentException("Not KeySpace");

//        // Add node with data
//        var rec = new TestContractRecord("marko", 29);
//        var recBase64 = rec.ToJson().ToBase64();

//        var testPath = keySpace.KeySystem.PathBuilder("nodes/node2/node2___contract.json");
//        string dataFilePath = "basepath/data/9c/a8/nodes/node2/node2___contract.json";
//        (testPath == dataFilePath).BeTrue();

//        var addResult = (await graphClient.Execute($"add node key=node2 set contract {{ '{recBase64}' }};")).BeOk();
//        graphEngine.DataManager.GetMap().Nodes.Count.Be(1);
//        graphEngine.DataManager.GetMap().Edges.Count.Be(0);
//        await CheckFileStoreCount(keyStore, 1);

//        // Verify data was written correctly
//        var readDataOption = (await keyStore.Get(dataFilePath)).BeOk();
//        TestContractRecord readRec = readDataOption.Return().ToObject<TestContractRecord>();
//        readRec.NotNull();
//        readRec.Name.Be(rec.Name);
//        readRec.Age.Be(rec.Age);

//        // Remove data from node and verify
//        var removeData = (await graphClient.Execute("set node key=node2 set -contract;")).BeOk();
//        await CheckFileStoreCount(keyStore, 0);
//        graphEngine.DataManager.GetMap().Nodes.Count.Be(1);
//        graphEngine.DataManager.GetMap().Edges.Count.Be(0);

//        // Verify data has been deleted
//        (await keyStore.Get(dataFilePath)).BeNotFound();

//        (await graphClient.Execute("select (key=node2);")).Action(x =>
//        {
//            x.BeOk();
//            x.Return().Action(y =>
//            {
//                y.Nodes.Count.Be(1);
//                y.Nodes[0].DataMap.Count.Be(0);
//                y.Edges.Count.Be(0);
//                y.DataLinks.Count.Be(0);
//            });
//        });

//        // Verify data map has been updated
//        var selectResultOption = (await graphClient.Execute("select (key=node2) return contract;")).BeOk();
//        selectResultOption.Return().Action(x =>
//        {
//            x.Nodes.Count.Be(1);
//            x.Nodes[0].DataMap.Count.Be(0);
//            x.Edges.Count.Be(0);
//            x.DataLinks.Count.Be(0);
//        });
//    }

//    [Theory]
//    [InlineData(false)]
//    [InlineData(true)]
//    public async Task AddNodeWithTwoData(bool useDataLake)
//    {
//        using var host = await CreateService(useDataLake);
//        var context = host.Services.GetRequiredService<ILogger<GraphEngineNodeDataTests>>();
//        var graphClient = host.Services.GetRequiredService<IGraphClient>();
//        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();
//        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();
//        KeySpace keySpace = keyStore as KeySpace ?? throw new ArgumentException("Not KeySpace");

//        var contractTestPath = keySpace.KeySystem.PathBuilder("nodes/node1/node1___contract.json");
//        string contractPath = "basepath/data/d1/da/nodes/node1/node1___contract.json";
//        (contractTestPath == contractPath).BeTrue();

//        var leaseTestPath = keySpace.KeySystem.PathBuilder("nodes/node1/node1___lease.json");
//        string leasePath = "basepath/data/f8/06/nodes/node1/node1___lease.json";
//        (leaseTestPath == leasePath).BeTrue();

//        var contractRec = new TestContractRecord("marko", 29);
//        var contractBase64 = contractRec.ToJson().ToBase64();
//        var leaseRec = new TestLeaseRecord("lease#1", 100.0m);
//        var leaseBase64 = leaseRec.ToJson().ToBase64();

//        var addResult = await graphClient.Execute($"add node key=node1 set lease {{ '{leaseBase64}' }}, contract {{ '{contractBase64}' }};");
//        addResult.BeOk();
//        graphEngine.DataManager.GetMap().Nodes.Count.Be(1);
//        graphEngine.DataManager.GetMap().Edges.Count.Be(0);
//        await CheckFileStoreCount(keyStore, 2);

//        (await keyStore.Get(contractPath)).Action((Action<Option<DataETag>>)(x =>
//        {
//            x.BeOk();
//            TestContractRecord readRec = x.Return().ToObject<TestContractRecord>();
//            readRec.NotNull();
//            readRec.Name.Be(contractRec.Name);
//            readRec.Age.Be(contractRec.Age);
//        }));

//        (await keyStore.Get(leasePath)).Action(x =>
//        {
//            x.BeOk();
//            TestLeaseRecord readRec = x.Return().ToObject<TestLeaseRecord>();
//            readRec.NotNull();
//            readRec.LeaseId.Be(leaseRec.LeaseId);
//            readRec.Amount.Be(leaseRec.Amount);
//        });

//        var selectResultOption = (await graphClient.Execute("select (key=node1) return contract;")).BeOk();

//        var selectResult = selectResultOption.Return();
//        selectResult.Nodes.Count.Be(1);
//        selectResult.Nodes[0].Key.Be("node1");
//        selectResult.Edges.Count.Be(0);
//        selectResult.DataLinks.Count.Be(1);
//        selectResult.DataLinks.Get("contract").ThrowOnError().Return().Action(x =>
//        {
//            TestContractRecord readRec = x.Data.ToObject<TestContractRecord>();
//            readRec.NotNull();
//            readRec.Name.Be(contractRec.Name);
//            readRec.Age.Be(contractRec.Age);
//        });

//        selectResultOption = (await graphClient.Execute("select (key=node1) return contract, lease;")).BeOk();

//        selectResult = selectResultOption.Return();
//        selectResult.Nodes.Count.Be(1);
//        selectResult.Nodes[0].Key.Be("node1");
//        selectResult.Edges.Count.Be(0);
//        selectResult.DataLinks.Count.Be(2);

//        selectResult.DataLinks.Get("contract").ThrowOnError().Return().Action(x =>
//        {
//            TestContractRecord readRec = x.Data.ToObject<TestContractRecord>();
//            readRec.NotNull();
//            readRec.Name.Be(contractRec.Name);
//            readRec.Age.Be(contractRec.Age);
//        });

//        selectResult.DataLinks.Get("lease").ThrowOnError().Return().Action(x =>
//        {
//            TestLeaseRecord readRec = x.Data.ToObject<TestLeaseRecord>();
//            readRec.NotNull();
//            readRec.LeaseId.Be(leaseRec.LeaseId);
//            readRec.Amount.Be(leaseRec.Amount);
//        });

//        var deleteResult = await graphClient.Execute("delete node key=node1;");
//        deleteResult.BeOk();

//        await CheckFileStoreCount(keyStore, 0);
//        graphEngine.DataManager.GetMap().Nodes.Count.Be(0);
//        graphEngine.DataManager.GetMap().Edges.Count.Be(0);

//        selectResultOption = (await graphClient.Execute("select (key=node1) return contract;")).BeOk();

//        selectResult = selectResultOption.Return();
//        selectResult.Nodes.Count.Be(0);
//        selectResult.Edges.Count.Be(0);
//        selectResult.DataLinks.Count.Be(0);
//    }


//    [Theory]
//    [InlineData(false)]
//    [InlineData(true)]
//    public async Task AddNodeWithTwoDataDeletingOne(bool useDataLake)
//    {
//        using var host = await CreateService(useDataLake);
//        var context = host.Services.GetRequiredService<ILogger<GraphEngineNodeDataTests>>();
//        var graphClient = host.Services.GetRequiredService<IGraphClient>();
//        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();
//        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();
//        KeySpace keySpace = keyStore as KeySpace ?? throw new ArgumentException("Not KeySpace");

//        var contractTestPath = keySpace.KeySystem.PathBuilder("nodes/node1/node1___contract.json");
//        string contractPath = "basepath/data/d1/da/nodes/node1/node1___contract.json";
//        (contractTestPath == contractPath).BeTrue();

//        var leaseTestPath = keySpace.KeySystem.PathBuilder("nodes/node1/node1___lease.json");
//        string leasePath = "basepath/data/f8/06/nodes/node1/node1___lease.json";
//        (leaseTestPath == leasePath).BeTrue();

//        var contractRec = new TestContractRecord("marko", 29);
//        var contractBase64 = contractRec.ToJson().ToBase64();
//        var leaseRec = new TestLeaseRecord("lease#1", 100.0m);
//        var leaseBase64 = leaseRec.ToJson().ToBase64();

//        var addResult = await graphClient.Execute($"add node key=node1 set lease {{ '{leaseBase64}' }}, contract {{ '{contractBase64}' }};");
//        addResult.BeOk();
//        graphEngine.DataManager.GetMap().Nodes.Count.Be(1);
//        graphEngine.DataManager.GetMap().Edges.Count.Be(0);
//        await CheckFileStoreCount(keyStore, 2);

//        (await keyStore.Get(contractPath)).Action(x =>
//        {
//            x.BeOk();
//            TestContractRecord readRec = x.Return().ToObject<TestContractRecord>();
//            readRec.NotNull();
//            readRec.Name.Be(contractRec.Name);
//            readRec.Age.Be(contractRec.Age);
//        });

//        (await keyStore.Get(leasePath)).Action(x =>
//        {
//            x.BeOk();
//            TestLeaseRecord readRec = x.Return().ToObject<TestLeaseRecord>();
//            readRec.NotNull();
//            readRec.LeaseId.Be(leaseRec.LeaseId);
//            readRec.Amount.Be(leaseRec.Amount);
//        });

//        // Delete data "contract" and verify data was deleted
//        var removeDataOption = await graphClient.Execute("set node key=node1 set -contract, t2=v2;");
//        removeDataOption.BeOk(removeDataOption.ToString());
//        await CheckFileStoreCount(keyStore, 1);
//        graphEngine.DataManager.GetMap().Nodes.Count.Be(1);
//        graphEngine.DataManager.GetMap().Nodes["node1"].Action(x =>
//        {
//            x.DataMap.Count.Be(1);
//            x.Tags.Count.Be(1);
//            x.Tags.TryGetValue("t2", out var value).BeTrue();
//            value.Be("v2");
//        });
//        graphEngine.DataManager.GetMap().Edges.Count.Be(0);

//        // Verify data has been deleted
//        (await keyStore.Get(contractPath)).IsNotFound().BeTrue();

//        // Verify data should still exist
//        (await keyStore.Get(leasePath)).Action(x =>
//        {
//            x.BeOk();
//            TestLeaseRecord readRec = x.Return().ToObject<TestLeaseRecord>();
//            readRec.NotNull();
//            readRec.LeaseId.Be(leaseRec.LeaseId);
//            readRec.Amount.Be(leaseRec.Amount);
//        });

//        // Verify node
//        (await graphClient.Execute("select (key=node1);")).Action(x =>
//        {
//            x.BeOk();
//            x.Return().Action(y =>
//            {
//                y.Nodes.Count.Be(1);
//                y.Nodes[0].DataMap.Count.Be(1);
//                y.Nodes[0].DataMap.ContainsKey("lease").BeTrue();
//            });
//        });

//        var deleteResult = (await graphClient.Execute("delete node key=node1;")).BeOk();

//        await CheckFileStoreCount(keyStore, 0);
//        graphEngine.DataManager.GetMap().Nodes.Count.Be(0);
//        graphEngine.DataManager.GetMap().Edges.Count.Be(0);

//        var selectResultOption = (await graphClient.Execute("select (key=node1) return contract;")).BeOk();
//        selectResultOption.Return().Action(x =>
//        {
//            x.Nodes.Count.Be(0);
//            x.Edges.Count.Be(0);
//            x.DataLinks.Count.Be(0);
//        });
//    }

//    private async Task CheckFileStoreCount(IKeyStore keyStore, int count)
//    {
//        (await keyStore.Search($"basepath/{GraphConstants.Data.BasePath}/*/*/nodes/**/*")).Count.Be(count);
//    }
//}
