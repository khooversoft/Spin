using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Types;

namespace Toolbox.Orleans.Serialization;

[GenerateSerializer]
public struct ScopeContext_Surrogate
{
    [Id(0)] public string TraceId;
}


[RegisterConverter]
public sealed class ScopeContext_SurrogateConverter : IConverter<ScopeContext, ScopeContext_Surrogate>
{
    public ScopeContext ConvertFromSurrogate(in ScopeContext_Surrogate surrogate) => new ScopeContext(surrogate.TraceId, NullLogger.Instance);

    public ScopeContext_Surrogate ConvertToSurrogate(in ScopeContext value) => new ScopeContext_Surrogate
    {
        TraceId = value.TraceId,
    };
}