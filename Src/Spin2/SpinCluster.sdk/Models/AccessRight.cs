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
    [Id(0)] public string Privilege { get; init; } = null!;
    [Id(1)] public string PrincipalId { get; init; } = null!;
    [Id(2)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;

    public bool Equals(AccessRight? obj) => obj is AccessRight document &&
        Privilege == document.Privilege &&
        PrincipalId == document.PrincipalId &&
        CreatedDate == document.CreatedDate;

    public override int GetHashCode() => HashCode.Combine(Privilege, PrincipalId, CreatedDate);
}


public static class AccessRightExtensions
{
    public static Option CanAccess(this IEnumerable<AccessRight> list, string privilege, PrincipalId principalId)
    {
        list.NotNull();
        privilege.NotEmpty();
        principalId.NotNull();

        return list.Any(x => x.Privilege == privilege && x.PrincipalId == principalId) switch
        {
            true => new Option(StatusCode.OK),
            false => new Option(StatusCode.Unauthorized),
        };
    }
}
