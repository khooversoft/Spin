using Toolbox.Tools;
using Toolbox.Extensions;

namespace Toolbox.Store;


public record ListKeySystem<T> : KeySystemBase
{
    private readonly Func<T, string> _serialize;
    private readonly Func<string, T?> _deserialize;

    public ListKeySystem(string basePath)
        : base(basePath, KeySystemType.List)
    {
        _serialize = obj => obj.ToJson();
        _deserialize = obj => obj.ToObject<T>();
    }

    public ListKeySystem(string basePath, SpaceSerializer? spaceSerializer)
        : base(basePath, KeySystemType.List)
    {
        if( spaceSerializer == null )
        {
            _serialize = obj => obj.ToJson();
            _deserialize = obj => obj.ToObject<T>();
            return;
        }

        _serialize = (T x) => spaceSerializer.Serializer(x.NotNull());

        _deserialize = x => spaceSerializer.Deserializer(x) switch
        {
            null => default,
           T v => v,
           _ => throw new ArgumentException($"Deserialized object is not of type {typeof(T).FullName}"),
        };
    }

    public ListKeySystem(string basePath, Func<T, string>? serialize, Func<string, T?>? deserialize)
        : base(basePath, KeySystemType.List)
    {
        _serialize = serialize ?? (obj => obj.ToJson());
        _deserialize = deserialize ?? (obj => obj.ToObject<T>());
    }

    public string Serialize(T subject) => _serialize(subject);

    public T? Deserialize(string data) => _deserialize(data);

    public string PathBuilder(string key)
    {
        key.NotEmpty();
        DateTime now = DateTime.UtcNow;
        var result = $"{CreatePathPrefix()}/{key}/{now:yyyyMM}/{key}-{now:yyyyMMdd-HHmmss}.{typeof(T).Name}.json";
        return result.ToLowerInvariant();
    }

    public DateTime ExtractTimeIndex(string path) => PartitionSchemas.ExtractTimeIndex(path);
}
