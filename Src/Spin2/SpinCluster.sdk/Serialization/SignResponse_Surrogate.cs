using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Data;
using Toolbox.Security.Sign;

namespace SpinCluster.sdk.Serialization;

[GenerateSerializer, Immutable]
public struct SignResponse_Surrogate
{
    [Id(0)] public string Id;
    [Id(1)] public string PrincipleId;
    [Id(2)] public string Kid;
    [Id(3)] public string MessageDigest;
    [Id(4)] public string JwtSignature;
}


[RegisterConverter]
public sealed class SignResponse_SurrogateConverter : IConverter<SignResponse, SignResponse_Surrogate>
{
    public SignResponse ConvertFromSurrogate(in SignResponse_Surrogate surrogate) => new SignResponse
    {
        Id = surrogate.Id,
        PrincipleId = surrogate.PrincipleId,
        Kid = surrogate.Kid,
        MessageDigest = surrogate.MessageDigest,
        JwtSignature = surrogate.JwtSignature,
    };

    public SignResponse_Surrogate ConvertToSurrogate(in SignResponse value) => new SignResponse_Surrogate
    {
        Id = value.Id,
        PrincipleId = value.PrincipleId,
        Kid = value.Kid,
        MessageDigest = value.MessageDigest,
        JwtSignature = value.JwtSignature,
    };
}
