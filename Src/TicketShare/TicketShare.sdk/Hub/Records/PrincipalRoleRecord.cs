using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public sealed record PrincipalRoleRecord
{
    public string PrincipalId { get; init; } = null!;
    public ChannelRole Role { get; init; } = ChannelRole.Reader;
    public string? LastMessageIdRead { get; init; }

    public bool Equals(PrincipalRoleRecord? obj) => obj is PrincipalRoleRecord subject &&
        PrincipalId == subject.PrincipalId &&
        Role == subject.Role &&
        LastMessageIdRead == subject.LastMessageIdRead;

    public override int GetHashCode() => HashCode.Combine(PrincipalId, Role, LastMessageIdRead);

    public static IValidator<PrincipalRoleRecord> Validator { get; } = new Validator<PrincipalRoleRecord>()
        .RuleFor(x => x.PrincipalId).NotEmpty()
        .RuleFor(x => x.Role).ValidEnum()
        .Build();
}

public static class PrincipalChannelTool
{
    public static Option Validate(this PrincipalRoleRecord subject) => PrincipalRoleRecord.Validator.Validate(subject).ToOptionStatus();

    public static bool IsRead(this PrincipalRoleRecord subject, string messageId) => subject.LastMessageIdRead?.CompareTo(messageId) >= 0;

    public static bool HasAccess(this PrincipalRoleRecord subject, ChannelRole requiredAccess) => requiredAccess switch
    {
        ChannelRole.Reader => true,
        ChannelRole.Contributor => subject.Role == ChannelRole.Contributor || subject.Role == ChannelRole.Owner,
        ChannelRole.Owner => subject.Role == ChannelRole.Owner,

        _ => throw new ArgumentException($"Unknown required access: {requiredAccess}")
    };
}
