using System.Collections.Frozen;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public sealed record PrincipalChannelRecord
{
    public string PrincipalId { get; init; } = null!;
    public ChannelRole Role { get; init; } = ChannelRole.Reader;
    public IReadOnlyDictionary<string, MessageStateRecord> MessageStates { get; init; } = FrozenDictionary<string, MessageStateRecord>.Empty;

    public bool Equals(PrincipalChannelRecord? obj) => obj is PrincipalChannelRecord subject &&
        PrincipalId == subject.PrincipalId &&
        Role == subject.Role &&
        MessageStates.DeepEquals(subject.MessageStates);

    public override int GetHashCode() => HashCode.Combine(PrincipalId, Role, MessageStates);

    public static IValidator<PrincipalChannelRecord> Validator { get; } = new Validator<PrincipalChannelRecord>()
        .RuleFor(x => x.PrincipalId).NotEmpty()
        .RuleFor(x => x.Role).ValidEnum()
        .RuleForEach(x => x.MessageStates.Values).Validate(MessageStateRecord.Validator)
        .Build();
}

public static class PrincipalChannelTool
{
    public static Option Validate(this PrincipalChannelRecord subject) => PrincipalChannelRecord.Validator.Validate(subject).ToOptionStatus();

    public static bool IsRead(this PrincipalChannelRecord subject, string messageId) => subject.MessageStates.TryGetValue(messageId, out MessageStateRecord? _);
}
