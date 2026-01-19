using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Store;

public record SpaceOption<T>
{
    public Func<T, string> Serializer { get; set; } = x => x.ToJson();
    public Func<string, T?> Deserializer { get; set; } = x => x.ToObject<T>();
}
