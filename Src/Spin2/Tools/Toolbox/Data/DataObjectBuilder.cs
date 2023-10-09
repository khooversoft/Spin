using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Data;

public class DataObjectBuilder
{
    public string? Key { get; set; }
    public string? TypeName { get; set; }
    public string? JsonData { get; set; }

    public DataObjectBuilder SetKey(string key) => this.Action(x => x.Key = key);
    public DataObjectBuilder SetTypeName(string typeName) => this.Action(x => x.TypeName = typeName);

    public DataObjectBuilder SetContent<T>(T value) where T : class
    {
        TypeName = typeof(T).GetTypeName();
        JsonData = value.ToJson();

        return this;
    }

    public DataObject Build()
    {
        TypeName.NotEmpty();
        JsonData.NotEmpty();

        Key ??= TypeName;
        Key.NotEmpty();

        return new DataObject
        {
            Key = Key,
            TypeName = TypeName,
            JsonData = JsonData,
        };
    }
}
