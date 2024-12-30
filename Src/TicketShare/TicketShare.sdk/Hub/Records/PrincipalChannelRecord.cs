using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public sealed record PrincipalChannelRecord
{
    public string PrincipalId { get; init; } = null!;
    public ChannelRole Role { get; init; } = ChannelRole.Reader;
    public IReadOnlyList<ReadMessageRecord> ReadMessageIds { get; init; } = Array.Empty<ReadMessageRecord>();

    public DateTime? IsRead(string messageId) => ReadMessageIds
        .Where(x => x.MessageId.EqualsIgnoreCase(messageId))
        .FirstOrDefault()?.ReadDate;

    public bool Equals(PrincipalChannelRecord? obj) => obj is PrincipalChannelRecord subject &&
        PrincipalId == subject.PrincipalId &&
        Role == subject.Role &&
        ReadMessageIds.OrderBy(x => x.MessageId).SequenceEqual(subject.ReadMessageIds.OrderBy(x => x.MessageId));

    public override int GetHashCode() => HashCode.Combine(PrincipalId, Role, ReadMessageIds);

    public static IValidator<PrincipalChannelRecord> Validator { get; } = new Validator<PrincipalChannelRecord>()
        .RuleFor(x => x.PrincipalId).NotEmpty()
        .RuleFor(x => x.Role).ValidEnum()
        .RuleForEach(x => x.ReadMessageIds).Validate(ReadMessageRecord.Validator)
        .Build();
}

public static class PrincipalChannelTool
{
    public static Option Validate(this PrincipalChannelRecord subject) => PrincipalChannelRecord.Validator.Validate(subject).ToOptionStatus();
}
