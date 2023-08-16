using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Data;
using Toolbox.Types;

namespace SpinCluster.sdk.Serialization;

public struct ScopeContextSerialization
{
    public string TraceId;
}


[RegisterConverter]
public sealed class ScopeContextSerializationConverter : IConverter<ScopeContext, ScopeContextSerialization>
{
    public ScopeContext ConvertFromSurrogate(in ScopeContextSerialization surrogate) => new ScopeContext(surrogate.TraceId);

    public ScopeContextSerialization ConvertToSurrogate(in ScopeContext value) => new ScopeContextSerialization
    {
        TraceId = value.TraceId,
    };
}
