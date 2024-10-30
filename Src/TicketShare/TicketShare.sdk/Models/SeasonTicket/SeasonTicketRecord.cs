using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public sealed record SeasonTicketRecord : IEquatable<SeasonTicketRecord>
{
    public string SeasonTicketId { get; init; } = null!;   // seasonTicket:{id}
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string OwnerPrincipalId { get; init; } = null!;
    public string? Tags { get; init; }
    public IReadOnlyList<RoleRecord> Members { get; init; } = Array.Empty<RoleRecord>();
    public IReadOnlyList<SeatRecord> Seats { get; init; } = Array.Empty<SeatRecord>();
    public IReadOnlyList<ChangeLog> ChangeLogs { get; init; } = Array.Empty<ChangeLog>();

    public bool Equals(SeasonTicketRecord? other) =>
        other != null &&
        SeasonTicketId == other.SeasonTicketId &&
        Name == other.Name &&
        Description == other.Description &&
        OwnerPrincipalId == other.OwnerPrincipalId &&
        Tags == other.Tags &&
        Enumerable.SequenceEqual(Members.OrderBy(x => x.PrincipalId), other.Members.OrderBy(x => x.PrincipalId)) &&
        Enumerable.SequenceEqual(Seats.OrderBy(x => x.SeatId), other.Seats.OrderBy(x => x.SeatId)) &&
        Enumerable.SequenceEqual(ChangeLogs.OrderBy(x => x.Date), other.ChangeLogs.OrderBy(x => x.Date));

    public override int GetHashCode() => HashCode.Combine(SeasonTicketId, Name, Description, OwnerPrincipalId, Tags);

    public static IValidator<SeasonTicketRecord> Validator { get; } = new Validator<SeasonTicketRecord>()
        .RuleFor(x => x.SeasonTicketId).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.OwnerPrincipalId).NotEmpty()
        .RuleForEach(x => x.Members).Validate(RoleRecord.Validator)
        .RuleForEach(x => x.Seats).Validate(SeatRecord.Validator)
        .RuleForEach(x => x.ChangeLogs).Validate(ChangeLog.Validator)
        .Build();
}


public static class SeasonTicketRecordTool
{
    public static Option Validate(this SeasonTicketRecord subject) => SeasonTicketRecord.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this SeasonTicketRecord subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static string ToSeasonTicketKey(string id) => $"seasonTicket:{id.NotEmpty().ToLower()}";
}