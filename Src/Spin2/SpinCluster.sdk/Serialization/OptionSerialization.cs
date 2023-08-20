﻿using Toolbox.Types;

namespace SpinCluster.sdk.Serialization;

[GenerateSerializer]
public struct OptionSerialization
{
    [Id(0)] public StatusCode StatusCode;
    [Id(1)] public string? Error;
}


[RegisterConverter]
public sealed class OptionSerializationConverter : IConverter<Option, OptionSerialization>
{
    public Option ConvertFromSurrogate(in OptionSerialization surrogate) => new Option(surrogate.StatusCode, surrogate.Error);

    public OptionSerialization ConvertToSurrogate(in Option value) => new OptionSerialization
    {
        StatusCode = value.StatusCode,
        Error = value.Error,
    };
}
