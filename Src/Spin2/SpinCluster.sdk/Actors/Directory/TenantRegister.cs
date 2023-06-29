using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Actors.ActorBase;

namespace SpinCluster.sdk.Actors.Directory;

[GenerateSerializer, Immutable]
public record TenantRegister
{
    [Id(0)] public string UserId { get; init; } = null!;
    [Id(1)] public string GlobalPrincipleId { get; init; } = Guid.NewGuid().ToString();
    [Id(2)] public string TenantName { get; init; } = null!;
    [Id(4)] public string Contact { get; init; } = null!;
    [Id(5)] public string? Email { get; init; }
    [Id(6)] public IReadOnlyList<UserPhone> Phone { get; init; } = Array.Empty<UserPhone>();
    [Id(7)] public IReadOnlyList<UserAddress> Addresses { get; init; } = Array.Empty<UserAddress>();

    [Id(8)] public IReadOnlyList<DataObject> DataObjects { get; init; } = Array.Empty<DataObject>();
    [Id(9)] public bool AccountEnabled { get; init; } = false;
    [Id(10)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(11)] public DateTime? ActiveDate { get; init; }
}
