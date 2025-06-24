using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Data.Client.Common;

public class DataJournalCommonTests
{
    public static async Task SingleAppend(IHost host)
    {
        host.NotNull();
        const string key = nameof(SingleAppend);

        IJournalClient<EntityModel> dataHandler = host.Services.GetJournalClient<EntityModel>();
        var context = host.Services.CreateContext<EntityModel>();
        await dataHandler.Delete(context);

        EntityModel entity = new() { Name = "First", Age = 25 };
        var writeOption = await dataHandler.Append([entity], context);
        writeOption.BeOk();

        var readOption = await dataHandler.Get(context);
        readOption.BeOk();

        (readOption.Return().First() == entity).BeTrue();
    }

    public static async Task TwoAppend(IHost host)
    {
        host.NotNull();
        const string key = nameof(SingleAppend);

        IJournalClient<EntityModel> dataHandler = host.Services.GetJournalClient<EntityModel>();
        var context = host.Services.CreateContext<EntityModel>();
        await dataHandler.Delete(context);

        var list = Enumerable.Range(0, 2)
            .Select(x => new EntityModel { Name = $"First-{x}", Age = 25 + x })
            .ToArray();
        list.Length.Be(2);

        var writeOption = await dataHandler.Append(list, context);
        writeOption.BeOk();

        var readOption = await dataHandler.Get(context);
        readOption.BeOk();
        readOption.Return().Action(x =>
        {
            x.Count.Be(2);
            Enumerable.SequenceEqual(list, x).BeTrue();
        });
    }

    public static async Task ScaleAppend(IHost host)
    {
        host.NotNull();
        const string key = nameof(SingleAppend);

        IJournalClient<EntityModel> dataHandler = host.Services.GetJournalClient<EntityModel>();
        var context = host.Services.CreateContext<EntityModel>();
        await dataHandler.Delete(context);

        int count = 10;
        int batchCount = 10;
        var fullList = new Sequence<EntityModel>();

        for (int i = 0; i < count; i++)
        {
            var list = Enumerable.Range(0, batchCount)
                .Select(x => new EntityModel { Name = $"First-{x}", Age = 25 + x })
                .ToArray();

            fullList += list;

            var writeOption = await dataHandler.Append(list, context);
            writeOption.BeOk();

            var readOption = await dataHandler.Get(context);
            readOption.BeOk();
            readOption.Return().Action(x =>
            {
                x.Count.Be(fullList.Count);
                Enumerable.SequenceEqual(fullList, x).BeTrue();
            });
        }
    }

    public record EntityModel
    {
        public string Name { get; init; } = null!;
        public int Age { get; init; }
    }
}
