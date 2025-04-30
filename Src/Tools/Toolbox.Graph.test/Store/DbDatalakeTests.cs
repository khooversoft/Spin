using Toolbox.Graph.test.Application;
using Toolbox.Graph.test.Store.TestingCode;
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
        var (testClient, context) = await TestApplication.CreateDatalake<DbDatalakeTests>(_basePath, _outputHelper);
        using (testClient)
        {
            await LoadAndCheckpointTesting.EmptyDbSave(testClient, context);
        }
    }

    [Fact]
    public async Task SimpleMapDbRoundTrip()
    {
        var (testClient, context) = await TestApplication.CreateDatalake<DbDatalakeTests>(_basePath, _outputHelper);
        using (testClient)
        {
            await LoadAndCheckpointTesting.SimpleMapDbRoundTrip(testClient, context);
        }
    }

    [Fact]
    public async Task LoadInitialDatabase()
    {
        var (testClient, context) = await TestApplication.CreateDatalake<DbDatalakeTests>(_basePath, _outputHelper);
        using (testClient)
        {
            await LoadAndCheckpointTesting.LoadInitialDatabase(testClient, context);
        }
    }

    [Fact]
    public async Task AddNodeWithData()
    {
        var (testClient, context) = await TestApplication.CreateDatalake<DbDatalakeTests>(_basePath, _outputHelper);
        using (testClient)
        {
            await NodeDataTesting.AddNodeWithData(testClient, context);
        }
    }

    [Fact]
    public async Task AddNodeWithDataAndDeleteData()
    {
        var (testClient, context) = await TestApplication.CreateDatalake<DbDatalakeTests>(_basePath, _outputHelper);
        using (testClient)
        {
            await NodeDataTesting.AddNodeWithDataAndDeleteData(testClient, context);
        }
    }

    [Fact]
    public async Task AddNodeWithTwoData()
    {
        var (testClient, context) = await TestApplication.CreateDatalake<DbDatalakeTests>(_basePath, _outputHelper);
        using (testClient)
        {
            await NodeDataTesting.AddNodeWithTwoData(testClient, context);
        }
    }

    [Fact]
    public async Task AddNodeWithTwoDataDeletingOne()
    {
        var (testClient, context) = await TestApplication.CreateDatalake<DbDatalakeTests>(_basePath, _outputHelper);
        using (testClient)
        {
            await NodeDataTesting.AddNodeWithTwoDataDeletingOne(testClient, context);
        }
    }
}
