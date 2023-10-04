using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Data;

public class DataObjectBuilder
{
    public string? Key { get; set; }
    public string TypeName { get; set; } = ".property";
    public IDictionary<string, string> Values { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public DataObjectBuilder SetKey(string key) => this.Action(x => x.Key = key);
    public DataObjectBuilder SetTypeName(string typeName) => this.Action(x => x.TypeName = typeName);
    public DataObjectBuilder Add(string key, string value) => this.Action(x => x.Values.Add(key, value));

    public DataObjectBuilder SetContent<T>(T value) where T : class
    {
        TypeName = typeof(T).GetTypeName();

        var values = value.GetConfigurationValues();
        values.ForEach(x => Values[x.Key] = x.Value);

        return this;
    }

    public DataObject Build()
    {
        TypeName.NotEmpty();
        Key ??= TypeName;

        return new DataObject
        {
            Key = Key.NotEmpty(),
            TypeName = TypeName.NotEmpty(),
            Values = Values.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase),
        };
    }
}
