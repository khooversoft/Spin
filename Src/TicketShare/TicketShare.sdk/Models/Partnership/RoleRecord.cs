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

public record RoleRecord
{
    [Id(0)] public string RoleId { get; init; } = null!;
    [Id(1)] public RolePermission MemberRole { get; init; } = RolePermission.None;

    public static IValidator<RoleRecord> Validator { get; } = new Validator<RoleRecord>()
        .RuleFor(x => x.RoleId).NotEmpty()
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
