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

        EntityModel entity = new() { Name = "First", Age = 25 };
        var writeOption = await dataHandler.Append(key, [entity], context);
        writeOption.BeOk();

        var readOption = await dataHandler.Get(key, context);
        readOption.BeOk();

        (readOption.Return().First() == entity).BeTrue();
    }

    public static async Task TwoAppend(IHost host)
    {
        host.NotNull();
        const string key = nameof(SingleAppend);

        IJournalClient<EntityModel> dataHandler = host.Services.GetJournalClient<EntityModel>();
        var context = host.Services.CreateContext<EntityModel>();

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
    }

    public record EntityModel
    {
        public string Name { get; init; } = null!;
        public int Age { get; init; }
    }
}
