using Toolbox.Types;

namespace Toolbox.Orleans;

[GenerateSerializer]
public struct Option_Surrogate
{
    [Id(0)] public StatusCode StatusCode;
    [Id(1)] public string? Error;
}


[RegisterConverter]
public sealed class Option_SurrogateConverter : IConverter<Option, Option_Surrogate>
{
    public Option ConvertFromSurrogate(in Option_Surrogate surrogate) => new Option(surrogate.StatusCode, surrogate.Error);

    public Option_Surrogate ConvertToSurrogate(in Option value) => new Option_Surrogate
    {
        StatusCode = value.StatusCode,
        Error = value.Error,
    };
}
