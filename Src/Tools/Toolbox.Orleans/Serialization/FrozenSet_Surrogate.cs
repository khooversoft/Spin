using System.Collections.Frozen;

namespace Toolbox.Orleans;

[GenerateSerializer]
public struct FrozenSet_Surrogate<T>
{
    [Id(0)] public T[] Data;
}


[RegisterConverter]
public sealed class FrozenSet_SurrogateConverter<T> : IConverter<FrozenSet<T>, FrozenSet_Surrogate<T>>
{
    public FrozenSet<T> ConvertFromSurrogate(in FrozenSet_Surrogate<T> surrogate)
    {
        return surrogate.Data != null ? surrogate.Data.ToFrozenSet<T>() : FrozenSet<T>.Empty;
    }

    public FrozenSet_Surrogate<T> ConvertToSurrogate(in FrozenSet<T> value) => new FrozenSet_Surrogate<T>
    {
        Data = value.ToArray(),
    };
}