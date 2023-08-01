using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Models;

[GenerateSerializer, Immutable]
public sealed record AccessRight
{
    [Id(0)] public bool WriteGrant { get; init; }
    [Id(1)] public string? Claim { get; init; }
    [Id(2)] public string BlockType { get; init; } = null!;
    [Id(3)] public string PrincipalId { get; init; } = null!;    

    public bool Equals(AccessRight? obj) => obj is AccessRight document &&
        WriteGrant == document.WriteGrant &&
        Claim == document.Claim &&
        BlockType == document.BlockType &&
        PrincipalId == document.PrincipalId;

    public override int GetHashCode() => HashCode.Combine(WriteGrant, Claim, BlockType, PrincipalId);
}


public static class AccessRightExtensions
{
    public static Option CanAccess(this IEnumerable<AccessRight> list, string privilege, PrincipalId principalId)
    {
        list.NotNull();
        privilege.NotEmpty();
        principalId.NotNull();

        return list.Any(x => x.Claim == privilege && x.PrincipalId == principalId) switch
        {
            true => new Option(StatusCode.OK),
            false => new Option(StatusCode.Unauthorized),
        };
    }
}
