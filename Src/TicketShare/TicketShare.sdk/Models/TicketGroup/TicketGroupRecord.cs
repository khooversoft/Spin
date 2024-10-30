using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;


/// <summary>
/// Collection of tickets
/// </summary>
public sealed record TicketGroupRecord
{
    // Id = "ticketCollection:samTicket/2024/hockey
    public string TicketGroupId { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string OwnerPrincipalId { get; init; } = null!;
    public string? Tags { get; init; }
    public IReadOnlyList<RoleRecord> Roles { get; init; } = Array.Empty<RoleRecord>();
    public IReadOnlyList<SeatRecord> Seats { get; init; } = Array.Empty<SeatRecord>();
    public IReadOnlyList<ChangeLog> ChangeLogs { get; init; } = Array.Empty<ChangeLog>();

    public bool Equals(TicketGroupRecord? obj) => obj is TicketGroupRecord subject &&
        TicketGroupId == subject.TicketGroupId &&
        Name == subject.Name &&
        Description == obj.Description &&
        OwnerPrincipalId == obj.OwnerPrincipalId &&
        Tags == obj.Tags &&
        Enumerable.SequenceEqual(Roles, obj.Roles) &&
        Enumerable.SequenceEqual(Seats, obj.Seats) &&
        Enumerable.SequenceEqual(ChangeLogs, obj.ChangeLogs);

    public override int GetHashCode() => HashCode.Combine(TicketGroupId, Name, Description, OwnerPrincipalId, Tags);

    public static IValidator<TicketGroupRecord> Validator { get; } = new Validator<TicketGroupRecord>()
        .RuleFor(x => x.TicketGroupId).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.OwnerPrincipalId).NotEmpty()
        .RuleForEach(x => x.Roles).Validate(RoleRecord.Validator)
        .RuleForEach(x => x.Seats).Validate(SeatRecord.Validator)
        .RuleForEach(x => x.ChangeLogs).Validate(ChangeLog.Validator)
        .Build();
}


public static class TicketCollectionRecordTool
{
    public static Option Validate(this TicketGroupRecord subject) => TicketGroupRecord.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this TicketGroupRecord subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static string ToTicketGroupKey(string id) => $"ticketGroup:{id.NotEmpty().ToLowerInvariant()}";
}
