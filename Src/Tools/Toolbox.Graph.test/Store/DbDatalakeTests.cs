using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure;
using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.Graph.test.Command;
using Toolbox.Graph.test.Store.TestingCode;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Store;

public class DbDatalakeTests
{
    private readonly ITestOutputHelper _outputHelper;
    private const string _basePath = $"graphTesting-{nameof(DbDatalakeTests)}";
    public DbDatalakeTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    [Fact]
    public async Task EmptyDbSave()
    {
        await DeleteDb();

        var (testClient, context) = await TestApplication.CreateDatalake<DbDatalakeTests>(_basePath, _outputHelper);
        using (testClient)
        {
            await LoadAndCheckpointTesting.EmptyDbSave(testClient, context);
        }
    }

    [Fact]
    public async Task SimpleMapDbRoundTrip()
    {
        await DeleteDb();

        var (testClient, context) = await TestApplication.CreateDatalake<DbDatalakeTests>(_basePath, _outputHelper);
        using (testClient)
        {
            await LoadAndCheckpointTesting.SimpleMapDbRoundTrip(testClient, context);
        }
    }

    [Fact]
    public async Task LoadInitialDatabase()
    {
        var expectedMap = await saveDatabase(_outputHelper);

        var (testClient, context) = await TestApplication.CreateDatalake<DbDatalakeTests>(_basePath, _outputHelper, true);
        using (testClient)
        {
            IGraphEngine host = testClient.Services.GetRequiredService<IGraphEngine>();

            var compareMap = GraphCommandTools.CompareMap(expectedMap, testClient.Map, true);
            compareMap.Count.Be(0);
        }

        static async Task<GraphMap> saveDatabase(ITestOutputHelper output)
        {
            (IServiceProvider service, ScopeContext context) = TestApplication.CreateDatalakeDirect<DbDatalakeTests>(_basePath, output);

            IFileStore fileStore = service.GetRequiredService<IFileStore>();
            const int count = 5;

            var expectedMap = new GraphMap();
            Enumerable.Range(0, count).ForEach(x => expectedMap.Add(new GraphNode($"node-{x}")));
            Enumerable.Range(0, count - 1).ForEach(x => expectedMap.Add(new GraphEdge($"node-{x}", $"node-{x + 1}", "et")));

            string dbJson = expectedMap.ToSerialization().ToJson();
            (await fileStore.File(GraphConstants.MapDatabasePath).ForceSet(dbJson.ToDataETag(), context)).BeOk();

            return expectedMap;
        }
    }

    [Fact]
    public async Task AddNodeWithData()
    {
        await DeleteDb();

        var (testClient, context) = await TestApplication.CreateDatalake<DbDatalakeTests>(_basePath, _outputHelper);
        using (testClient)
        {
            await NodeDataTesting.AddNodeWithData(testClient, context);
        }
    }

    [Fact]
    public async Task AddNodeWithDataAndDeleteData()
    {
        await DeleteDb();

        var (testClient, context) = await TestApplication.CreateDatalake<DbDatalakeTests>(_basePath, _outputHelper);
        using (testClient)
        {
            await NodeDataTesting.AddNodeWithDataAndDeleteData(testClient, context);
        }
    }

    [Fact]
    public async Task AddNodeWithTwoData()
    {
        await DeleteDb();

        var (testClient, context) = await TestApplication.CreateDatalake<DbDatalakeTests>(_basePath, _outputHelper);
        using (testClient)
        {
            await NodeDataTesting.AddNodeWithTwoData(testClient, context);
        }
    }

    [Fact]
    public async Task AddNodeWithTwoDataDeletingOne()
    {
        await DeleteDb();

        var (testClient, context) = await TestApplication.CreateDatalake<DbDatalakeTests>(_basePath, _outputHelper);
        using (testClient)
        {
            await NodeDataTesting.AddNodeWithTwoDataDeletingOne(testClient, context);
        }
    }

    private async Task DeleteDb()
    {
        await TestApplication.CreateDatalakeDirect<DbDatalakeTests>(_basePath, _outputHelper).Func(async x =>
        {
            (await x.Service.GetRequiredService<IFileStore>().File(GraphConstants.MapDatabasePath).ForceDelete(x.Context)).BeOk();
        });
    }
}
