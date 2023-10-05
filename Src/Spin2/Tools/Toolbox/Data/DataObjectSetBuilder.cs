using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Data;

public class DataObjectSetBuilder
{
    public Dictionary<string, DataObject> Items { get; } = new Dictionary<string, DataObject>(StringComparer.OrdinalIgnoreCase);

    public DataObjectSetBuilder Add(DataObject dataObject, string? key = null)
    {
        Items[key ?? dataObject.Key] = dataObject;
        return this;
    }

    public DataObjectSetBuilder Add<T>(string key, T value) where T : class
    {
        key.NotEmpty();
        Items[key] = new DataObjectBuilder().SetContent(value).Build();
        return this;
    }

    public DataObjectSetBuilder Add<T>(T value) where T : class
    {
        var data = new DataObjectBuilder().SetContent(value).Build();
        Items[data.Key] = data;
        return this;
    }

    public DataObjectSet Build() => new DataObjectSet(Items);
}