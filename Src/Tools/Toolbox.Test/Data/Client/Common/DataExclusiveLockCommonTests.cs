using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Data.Client.Common;

public class DataExclusiveLockCommonTests
{
    public static async Task ExclusiveLock(IHost host, string pipelineName, string key)
    {
        IDataClient<EntityModel> dataClient = host.Services.GetDataClient<EntityModel>(pipelineName);
        var context = host.Services.CreateContext<DataExclusiveLockCommonTests>();
        IFileStore fileStore = host.Services.GetRequiredService<IFileStore>();
        DataPipelineContext dataContext = host.Services.GetDataPipelineBuilder<EntityModel>(pipelineName).CreateGet<EntityModel>(key);

        await dataClient.Delete(key, context).ConfigureAwait(false);

        var entityModel = new EntityModel
        {
            Name = "Test",
            Age = 30
        };

        (await dataClient.Set(key, entityModel, context).ConfigureAwait(false)).BeOk();

        // Verify path status is locked
        (await fileStore.File(dataContext.Path).GetDetails(context).ConfigureAwait(false)).Action(x =>
        {
            x.BeOk();
            var fileDetail = x.Return();
            fileDetail.LeaseStatus.Be(LeaseStatus.Locked);
            fileDetail.LeaseDuration.Be(LeaseDuration.Infinite);
        });

        // Verify that file is locked
        var dummyEntry = new EntityModel().ToDataETag();
        var readRawOption = await fileStore.File(dataContext.Path).Set(dummyEntry, context).ConfigureAwait(false);
        readRawOption.BeError();

        var readOption = await dataClient.Get(key, context).ConfigureAwait(false);
        readOption.BeOk();
        (readOption.Return() == entityModel).BeTrue();

        var unlockOption = await dataClient.ReleaseLock(key, context).ConfigureAwait(false);
        unlockOption.BeOk();

        (await fileStore.File(dataContext.Path).GetDetails(context).ConfigureAwait(false)).Action(x =>
        {
            x.BeOk();
            var fileDetail = x.Return();
            fileDetail.LeaseStatus.Be(LeaseStatus.Unlocked);
        });

        (await dataClient.Delete(key, context).ConfigureAwait(false)).BeOk();

        (await fileStore.File(dataContext.Path).Exists(context).ConfigureAwait(false)).Assert(x => x == StatusCode.NotFound);
    }

    internal record EntityModel
    {
        public string Name { get; init; } = null!;
        public int Age { get; init; }
    }
}
