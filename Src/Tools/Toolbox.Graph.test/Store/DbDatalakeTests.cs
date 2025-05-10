using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure;
using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.Graph.test.Store.TestingCode;
using Toolbox.Store;
using Toolbox.Tools;
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
