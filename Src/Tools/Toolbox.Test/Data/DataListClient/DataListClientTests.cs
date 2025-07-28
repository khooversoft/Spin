using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Data.Client;

public class DataListClientTests
{
    private readonly ITestOutputHelper _outputHelper;
    public DataListClientTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    protected virtual void AddStore(IServiceCollection services) => services.AddInMemoryFileStore();

    private async Task<IHost> BuildService(bool addMemory, bool addQueueStore, bool addListStore = true)
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(config => config.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(x => true));
                AddStore(services);

                services.AddDataListPipeline<EntityModel>(builder =>
                {
                    builder.MemoryCacheDuration = TimeSpan.FromMinutes(5);
                    builder.BasePath = nameof(DataListClientTests);

                    if (addMemory) builder.AddCacheMemory();
                    if (addQueueStore) builder.AddQueueStore();
                    if (addListStore) builder.AddListStore();
                });
            })
            .Build();

        await host.ClearStore<DataListClientTests>();
        return host;
    }


    [Fact]
    public async Task NoHandler()
    {
        using var host = await BuildService(false, false, false);

        Verify.Throw<ArgumentException>(() => host.Services.GetDataListClient<EntityModel>());
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task SingleItem(bool addMemory, bool addQueueStore)
    {
        const string key = nameof(SingleItem);
        var host = await BuildService(addMemory, addQueueStore);
        var context = host.Services.CreateContext<DataListClientTests>();
        var listClient = host.Services.GetDataListClient<EntityModel>();

        var entity = new EntityModel { Name = "Test", Age = 30 };
        var addOption = await listClient.Append(key, [entity], context);
        addOption.BeOk();

        var listOption = await listClient.Get(key, context);
        listOption.BeOk();
        var list = listOption.Return();
        list.Count.Be(1);
        (list[0] == entity).BeTrue();

        var searchOption = await listClient.Search(key, "**/*", context);
        searchOption.BeOk();
        var items = searchOption.Return();

        var deleteOption = await listClient.Delete(key, context);
        deleteOption.BeOk();

        var getOption = await listClient.Get(key, context);
        getOption.BeOk();
        getOption.Return().Count.Be(0);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task SingleSetItem(bool addMemory, bool addQueueStore)
    {
        const string key = nameof(SingleSetItem);
        var host = await BuildService(addMemory, addQueueStore);
        var context = host.Services.CreateContext<DataListClientTests>();
        var listClient = host.Services.GetDataListClient<EntityModel>();

        const int count = 10;

        var entities = Enumerable.Range(0, count)
            .Select(x => new EntityModel { Name = $"Test{x}", Age = 30 + x })
            .ToArray();

        var addOption = await listClient.Append(key, entities, context);
        addOption.BeOk();

        var listOption = await listClient.Get(key, context);
        listOption.BeOk();
        var list = listOption.Return();
        list.Count.Be(count);
        list.SequenceEqual(entities).BeTrue();

        var deleteOption = await listClient.Delete(key, context);
        deleteOption.BeOk();

        var getOption = await listClient.Get(key, context);
        getOption.BeOk();
        getOption.Return().Count.Be(0);
    }

    internal record EntityModel
    {
        public string Name { get; init; } = null!;
        public int Age { get; init; }
    }
}
