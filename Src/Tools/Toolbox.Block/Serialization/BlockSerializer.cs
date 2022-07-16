using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Block.Serialization;

public class BlockSerializer
{
    public static BlockSerializer Default { get; } = new BlockSerializer();

    private IReadOnlyList<DataItem> InternalSerialize<T>(T subject) where T : class
    {
        subject.NotNull();

        IReadOnlyList<KeyValuePair<string, string>> values = subject.GetConfigurationValues();

        return values
            .Select(x => new DataItem { DataType = subject.GetType().Name, Key = x.Key, Value = x.Value })
            .ToList();
    }

    private T InternalDeserialize<T>(IEnumerable<DataItem> dataItems) where T : class, new()
    {
        dataItems.NotNull();

        var value = new ConfigurationBuilder()
            .AddCommandLine(dataItems
                .Select(x => $"{x.Key}={x.Value}")
                .ToArray())
            .Build()
            .Bind<T>();

        return value;

        //var dict = dataItems.ToDictionary(x => x.Key, x => x.Value);

        //return new ConfigurationBuilder()
        //    .AddInMemoryCollection(dict)
        //    .Build()
        //    .Bind<T>();
    }

    public static IReadOnlyList<DataItem> Serialize<T>(T subject) where T : class => Default.InternalSerialize(subject);

    public static T Deserialize<T>(IEnumerable<DataItem> dataItems) where T : class, new() => Default.InternalDeserialize<T>(dataItems);
}
