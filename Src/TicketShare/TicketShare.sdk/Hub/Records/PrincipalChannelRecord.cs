using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public sealed record PrincipalChannelRecord
{
    public string PrincipalId { get; init; } = null!;
    public ChannelRole Role { get; init; } = ChannelRole.Reader;
    public string? LastMessageIdRead { get; init; }

    public bool Equals(PrincipalChannelRecord? obj) => obj is PrincipalChannelRecord subject &&
        PrincipalId == subject.PrincipalId &&
        Role == subject.Role &&
        LastMessageIdRead == subject.LastMessageIdRead;

    public override int GetHashCode() => HashCode.Combine(PrincipalId, Role, LastMessageIdRead);

    public static IValidator<PrincipalChannelRecord> Validator { get; } = new Validator<PrincipalChannelRecord>()
        .RuleFor(x => x.PrincipalId).NotEmpty()
        .RuleFor(x => x.Role).ValidEnum()
        .Build();
}

public static class PrincipalChannelTool
{
    public static Option Validate(this PrincipalChannelRecord subject) => PrincipalChannelRecord.Validator.Validate(subject).ToOptionStatus();

    public static bool IsRead(this PrincipalChannelRecord subject, string messageId) => subject.LastMessageIdRead?.CompareTo(messageId) >= 0;

    public static bool HasAccess(this PrincipalChannelRecord subject, ChannelRole requiredAccess) => requiredAccess switch
    {
        ChannelRole.Reader => true,
        ChannelRole.Contributor => subject.Role == ChannelRole.Contributor || subject.Role == ChannelRole.Owner,
        ChannelRole.Owner => subject.Role == ChannelRole.Owner,

        _ => throw new ArgumentException($"Unknown required access: {requiredAccess}")
    };
}
