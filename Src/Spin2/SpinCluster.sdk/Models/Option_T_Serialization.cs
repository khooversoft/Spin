using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Types;

namespace SpinCluster.sdk.Models;

[GenerateSerializer]
public struct Option_T_Serialization<T>
{
    [Id(0)] public StatusCode StatusCode;
    [Id(1)] public bool HasValue;
    [Id(2)] public T ValueObject;
    [Id(3)] public string? Error;
}


[RegisterConverter]
public sealed class Option_T_SerializationConverter<T> : IConverter<Option<T>, Option_T_Serialization<T>>
{
    public Option<T> ConvertFromSurrogate(in Option_T_Serialization<T> surrogate) => new Option<T>(surrogate.StatusCode, surrogate.Error);

    public Option_T_Serialization<T> ConvertToSurrogate(in Option<T> value) => new Option_T_Serialization<T>
    {
        StatusCode = value.StatusCode,
        Error = value.Error,
    };
}
