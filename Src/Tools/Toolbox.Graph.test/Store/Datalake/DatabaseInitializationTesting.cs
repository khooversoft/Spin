using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure;
using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.Graph.test.Command;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Store.Datalake;

public class DatabaseInitializationTesting
{
    const string _basePath = $"graphTesting-{nameof(DatabaseInitializationTesting)}";
    private readonly ITestOutputHelper _output;
    public DatabaseInitializationTesting(ITestOutputHelper output) => _output = output;

    [Fact]
    public async Task ExclusiveLockDbNotInitialized()
    {
        (IServiceProvider service, ScopeContext directContext) = TestApplication.CreateDatalakeDirect<DatabaseInitializationTesting>(_basePath, _output);

        IFileStore fileStore = service.GetRequiredService<IFileStore>().NotNull();
        (await fileStore.File(GraphConstants.MapDatabasePath).ForceDelete(directContext)).BeOk();

        var (testClient, context) = await TestApplication.CreateDatalake<DbDatalakeTests>(_basePath, _output);
        using var t1 = testClient;

        var select = (await testClient.Execute("select (*);", context)).BeOk().Return();
        select.Nodes.Count.Be(0);
        select.Edges.Count.Be(0);

        (await fileStore.File(GraphConstants.MapDatabasePath).Set("test".ToDataETag(), context)).BeError();
    }
}

public class LoadInitialDatabaseTest
{
    const string _basePath = $"graphTesting-{nameof(LoadInitialDatabaseTest)}";
    private readonly ITestOutputHelper _output;
    public LoadInitialDatabaseTest(ITestOutputHelper output) => _output = output;


    [Fact]
    public async Task LoadInitialDatabase()
    {
        var expectedMap = await saveDatabase(_output);

        var (testClient, context) = await TestApplication.CreateDatalake<DbDatalakeTests>(_basePath, _output, true);
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
}
