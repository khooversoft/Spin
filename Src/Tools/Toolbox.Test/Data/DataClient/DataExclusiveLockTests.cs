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

public class DataExclusiveLockTests
{
    private readonly ITestOutputHelper _outputHelper;
    private const string _basePath = "data/DataExclusiveTests";
    private const string _key = "exclusive-lock-key";

    public DataExclusiveLockTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    [Fact]
    public async Task ExclusiveLock()
    {
        using var host = await BuildService();

        IDataClient<EntityModel> dataClient = host.Services.GetDataClient<EntityModel>();
        var context = host.Services.CreateContext<DataExclusiveLockTests>();
        IFileStore fileStore = host.Services.GetRequiredService<IFileStore>();
        string path = host.Services.GetRequiredService<DataPipelineConfig<EntityModel>>().CreatePath<EntityModel>(_key);

        await dataClient.Delete(_key, context);

        var entityModel = new EntityModel
        {
            Name = "Test",
            Age = 30
        };

        (await dataClient.Set(_key, entityModel, context)).BeOk();

        // Verify path status is locked
        (await fileStore.File(path).GetDetails(context)).Action(x =>
        {
            x.BeOk();
            var fileDetail = x.Return();
            fileDetail.LeaseStatus.Be(LeaseStatus.Locked);
            fileDetail.LeaseDuration.Be(LeaseDuration.Infinite);
        });

        // Verify that file is locked
        var dummyEntry = new EntityModel().ToDataETag();
        (await fileStore.File(path).Set(dummyEntry, context)).IsLocked().BeTrue();

        (await dataClient.Get(_key, context)).Action(x =>
        {
            x.BeOk();
            (x.Return() == entityModel).BeTrue();
        });

        (await dataClient.ReleaseLock(_key, context)).BeOk();

        (await fileStore.File(path).GetDetails(context)).Action(x =>
        {
            x.BeOk();
            var fileDetail = x.Return();
            fileDetail.LeaseStatus.Be(LeaseStatus.Unlocked);
        });

        (await dataClient.Delete(_key, context)).BeOk();

        (await fileStore.File(path).Exists(context)).Assert(x => x == StatusCode.NotFound);
    }

    private record EntityModel
    {
        public string Name { get; init; } = null!;
        public int Age { get; init; }
    }

    protected virtual void AddStore(IServiceCollection services) => services.AddInMemoryFileStore();

    private async Task<IHost> BuildService()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(config => config.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(x => true));
                AddStore(services);

                services.AddDataPipeline<EntityModel>(builder =>
                {
                    builder.BasePath = _basePath;
                    builder.AddFileLocking(config =>
                    {
                        config.Add<EntityModel>(LockMode.Exclusive);
                    });

                    builder.AddFileStore();
                });
            })
            .Build();


        await host.ClearStore<DataListClientTests>();
        return host;
    }
}
