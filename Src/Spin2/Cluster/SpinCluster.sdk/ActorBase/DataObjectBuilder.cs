using Toolbox.Extensions;
using Toolbox.Tools;

namespace SpinCluster.sdk.ActorBase;

public class DataObjectBuilder
{
    public string? Key { get; set; }
    public string TypeName { get; set; } = ".property";
    public IList<KeyValuePair<string, string>> Values { get; set; } = new List<KeyValuePair<string, string>>();

    public DataObjectBuilder SetKey(string key) => this.Action(x => x.Key = key);
    public DataObjectBuilder SetTypeName(string typeName) => this.Action(x => x.TypeName = typeName);
    public DataObjectBuilder Add(string key, string value) => this.Action(x => x.Values.Add(new KeyValuePair<string, string>(key, value)));

    public DataObject Build()
    {
        Key.NotEmpty();
        TypeName.NotEmpty();

        return new DataObject
        {
            Key = Key.NotEmpty(),
            TypeName = TypeName.NotEmpty(),
            Values = Values.ToArray(),
        };
    }
}
