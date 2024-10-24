using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public enum RolePermission
{
    None,                   // Invalid    
    Owner,                  // Make any modifications, including adding a removing members    
    Contributor,            // Can propose agreement modifications, and accept from other    
    ReadOnly,               // Can only view
}

public sealed record RoleRecord : IEquatable<RoleRecord>
{
    public string PrincipalId { get; init; } = null!;
    public RolePermission MemberRole { get; init; } = RolePermission.None;

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

    public static bool Validate(this RoleRecord subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
