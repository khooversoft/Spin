using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Data;
using Toolbox.Security.Sign;

namespace SpinCluster.sdk.Serialization;

[GenerateSerializer]
public struct SignRequest_Surrogate
{
    [Id(0)] public string Id;
    [Id(1)] public string PrincipleId;
    [Id(2)] public string MessageDigest;
}


[RegisterConverter]
public sealed class SignRequest_SurrogateConverter : IConverter<SignRequest, SignRequest_Surrogate>
{
    public SignRequest ConvertFromSurrogate(in SignRequest_Surrogate surrogate) => new SignRequest
    {
        Id = surrogate.Id,
        PrincipalId = surrogate.PrincipleId,
        MessageDigest = surrogate.MessageDigest,
    };

    public SignRequest_Surrogate ConvertToSurrogate(in SignRequest value) => new SignRequest_Surrogate
    {
        Id = value.Id,
        PrincipleId = value.PrincipalId,
        MessageDigest = value.MessageDigest,
    };
}
