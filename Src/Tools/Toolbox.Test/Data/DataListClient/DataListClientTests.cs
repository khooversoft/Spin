using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
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
    [InlineData(false, false)]
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

        var listOption = await listClient.Get(key, "**/*", context);
        listOption.BeOk();
        var list = listOption.Return();
        list.Count.Be(1);
        (list[0] == entity).BeTrue();

        var searchOption = await listClient.Search(key, "**/*", context);
        searchOption.BeOk();
        var items = searchOption.Return().Action(x =>
        {
            x.Count.Be(1);
            x[0].Path.Contains(key + "-", StringComparison.OrdinalIgnoreCase).BeTrue();
            x[0].Path.EndsWith(typeof(EntityModel).Name + ".json", StringComparison.OrdinalIgnoreCase).BeTrue();
            x[0].IsFolder.BeFalse();
        });

        var deleteOption = await listClient.Delete(key, context);
        deleteOption.BeOk();

        var getOption = await listClient.Get(key, "**/*", context);
        getOption.BeOk();
        getOption.Return().Count.Be(0);
    }

    [Theory]
    [InlineData(false, false)]
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

        if (addQueueStore) (await listClient.Drain(context)).BeOk();

        var searchOption = await listClient.Search(key, "**/*", context);
        searchOption.BeOk();
        var items = searchOption.Return().Action(x =>
        {
            x.Count.Be(1);
            x[0].Path.Contains(key + "-", StringComparison.OrdinalIgnoreCase).BeTrue();
            x[0].Path.EndsWith(typeof(EntityModel).Name + ".json", StringComparison.OrdinalIgnoreCase).BeTrue();
            x[0].IsFolder.BeFalse();
        });

        var listOption = await listClient.Get(key, "**/*", context);
        listOption.BeOk();
        var list = listOption.Return();
        list.Count.Be(count);
        list.SequenceEqual(entities).BeTrue();

        var deleteOption = await listClient.Delete(key, context);
        deleteOption.BeOk();

        var getOption = await listClient.Get(key, "**/*", context);
        getOption.BeOk();
        getOption.Return().Count.Be(0);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task MultipleSetItem(bool addMemory, bool addQueueStore)
    {
        const string key = nameof(SingleSetItem);
        var host = await BuildService(addMemory, addQueueStore);
        var context = host.Services.CreateContext<DataListClientTests>();
        var listClient = host.Services.GetDataListClient<EntityModel>();

        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        const int batchSize = 10;
        var sourceList = new Sequence<EntityModel>();
        int batchCount = 0;

        while (!tokenSource.IsCancellationRequested)
        {
            context.LogDebug("Adding batch {count} with {BatchCount} items", batchCount++, batchSize);
            var entities = Enumerable.Range(0, batchSize)
                .Select(x => new EntityModel { Name = $"Test{x}", Age = 30 + x })
                .ToArray();

            sourceList += entities;

            var addOption = await listClient.Append(key, entities, context);
            addOption.BeOk();

            if (addQueueStore) (await listClient.Drain(context)).BeOk();

            var searchOption = await listClient.Search(key, "**/*", context);
            searchOption.BeOk();
            var items = searchOption.Return().Action(x =>
            {
                x.Count.Assert(x => x > 0, "Search should return at least one item");
                x.ForEach(y =>
                {
                    y.Path.Contains(key + "-", StringComparison.OrdinalIgnoreCase).BeTrue();
                    y.Path.EndsWith(typeof(EntityModel).Name + ".json", StringComparison.OrdinalIgnoreCase).BeTrue();
                    y.IsFolder.BeFalse();
                });
            });

            // Read all items and compare to source list
            var listOption = await listClient.Get(key, "**/*", context);
            listOption.BeOk();
            listOption.Return().Action(x =>
            {
                x.Count.Be(sourceList.Count);
                x.SequenceEqual(sourceList).BeTrue();
            });
        }

        context.LogInformation("Source list count={count}, batchCount={batchCount}", sourceList.Count, batchCount);
        sourceList.Count.Assert(x => x > 10, "Should have added more than 10 items");
        (await listClient.Delete(key, context)).BeOk();
        (await listClient.Get(key, "**/*", context)).BeOk().Return().Count.Be(0);
    }

    private record EntityModel
    {
        public string Name { get; init; } = null!;
        public int Age { get; init; }
    }
}
