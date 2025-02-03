using Toolbox.Graph.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public enum RoleType
{
    None,                   // Invalid    
    Owner,                  // Can add user and set roles, permissions
    Contributor,            // Can propose agreement modifications, and accept from other    
    Reader,                 // Can only view
}

public sealed record RoleRecord : IEquatable<RoleRecord>
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string PrincipalId { get; init; } = null!;
    public RoleType MemberRole { get; init; } = RoleType.None;

    public bool Equals(RoleRecord? other) =>
        other != null &&
        PrincipalId == other.PrincipalId &&
        MemberRole == other.MemberRole;

    public override int GetHashCode() => HashCode.Combine(PrincipalId, MemberRole);

    public static IValidator<RoleRecord> Validator { get; } = new Validator<RoleRecord>()
        .RuleFor(x => x.PrincipalId).NotEmpty()
        .RuleFor(x => x.MemberRole).ValidEnum()
        .Build();
}

public static class MembershipRecordExtensions
{
    public static Option Validate(this RoleRecord subject) => RoleRecord.Validator.Validate(subject).ToOptionStatus();

    public static SecurityAccess ToSecurityAccess(this RoleType subject) => subject switch
    {
        RoleType.Owner => SecurityAccess.Owner,
        RoleType.Contributor => SecurityAccess.Contributor,
        RoleType.Reader => SecurityAccess.Reader,
        _ => throw new InvalidOperationException("Invalid role type"),
    };
}
