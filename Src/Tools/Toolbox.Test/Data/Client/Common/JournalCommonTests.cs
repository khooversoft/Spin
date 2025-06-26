using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Data.Client.Common;

public class JournalCommonTests
{
    public static async Task SingleAppend(IHost host, string pipelineName)
    {
        host.NotNull();
        const string key = nameof(SingleAppend);

        IJournalClient<EntityModel> dataHandler = host.Services.GetJournalClient<EntityModel>(pipelineName);
        IFileStore fileStore = host.Services.GetRequiredService<IFileStore>();
        var context = host.Services.CreateContext<EntityModel>();
        DataPipelineContext dataContext = host.Services.GetDataPipelineBuilder<EntityModel>(pipelineName).CreateGet<EntityModel>(key);

        await dataHandler.Delete(key, context);

        EntityModel entity = new() { Name = "First", Age = 25 };
        var writeOption = await dataHandler.Append(key, [entity], context);
        writeOption.BeOk();

        var readOption = await dataHandler.Get(key, context);
        readOption.BeOk();

        (readOption.Return().First() == entity).BeTrue();

        string lookupName = key ?? typeof(EntityModel).Name;
        var lookupOption = await fileStore.File(dataContext.Path).Exists(context);
        lookupOption.BeOk();

        (await dataHandler.Delete(key, context)).BeOk();
    }

    public static async Task TwoAppend(IHost host, string pipelineName)
    {
        host.NotNull();
        const string key = nameof(TwoAppend);

        IJournalClient<EntityModel> dataHandler = host.Services.GetJournalClient<EntityModel>(pipelineName);
        IFileStore fileStore = host.Services.GetRequiredService<IFileStore>();
        var context = host.Services.CreateContext<EntityModel>();
        DataPipelineContext dataContext = host.Services.GetDataPipelineBuilder<EntityModel>(pipelineName).CreateGet<EntityModel>(key);

        await dataHandler.Delete(key, context);

        var list = Enumerable.Range(0, 2)
            .Select(x => new EntityModel { Name = $"First-{x}", Age = 25 + x })
            .ToArray();
        list.Length.Be(2);

        var writeOption = await dataHandler.Append(key, list, context);
        writeOption.BeOk();

        var readOption = await dataHandler.Get(key, context);
        readOption.BeOk();
        readOption.Return().Action(x =>
        {
            x.Count.Be(2);
            Enumerable.SequenceEqual(list, x).BeTrue();
        });

        string lookupName = pipelineName ?? typeof(EntityModel).Name;
        var lookupOption = await fileStore.File(dataContext.Path).Exists(context);
        lookupOption.BeOk();

        (await dataHandler.Delete(key, context)).BeOk();
    }

    public static async Task ScaleAppend(IHost host, string pipelineName)
    {
        host.NotNull();
        const string key = nameof(ScaleAppend);

        IJournalClient<EntityModel> dataHandler = host.Services.GetJournalClient<EntityModel>(pipelineName);
        IFileStore fileStore = host.Services.GetRequiredService<IFileStore>();
        var context = host.Services.CreateContext<EntityModel>();
        DataPipelineContext dataContext = host.Services.GetDataPipelineBuilder<EntityModel>(pipelineName).CreateGet<EntityModel>(key);

        await dataHandler.Delete(key, context);

        int count = 10;
        int batchCount = 10;
        var fullList = new Sequence<EntityModel>();

        for (int i = 0; i < count; i++)
        {
            var list = Enumerable.Range(0, batchCount)
                .Select(x => new EntityModel { Name = $"First-{x}", Age = 25 + x })
                .ToArray();

            fullList += list;

            var writeOption = await dataHandler.Append(key, list, context);
            writeOption.BeOk();

            var readOption = await dataHandler.Get(key, context);
            readOption.BeOk();
            readOption.Return().Action(x =>
            {
                x.Count.Be(fullList.Count);
                Enumerable.SequenceEqual(fullList, x).BeTrue();
            });
        }

        string lookupName = pipelineName ?? typeof(EntityModel).Name;
        var lookupOption = await fileStore.File(dataContext.Path).Exists(context);
        lookupOption.BeOk();

        (await dataHandler.Delete(key, context)).BeOk();
    }

    public record EntityModel
    {
        public string Name { get; init; } = null!;
        public int Age { get; init; }
    }
}
