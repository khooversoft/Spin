using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

[GenerateSerializer]
[Alias("TicketShare.sdk.SeatRecord")]
public sealed record SeatRecord : IEquatable<SeatRecord>
{
    [Id(0)] public string SeatId { get; init; } = null!;
    [Id(1)] public DateTime Date { get; init; } = DateTime.Now;
    [Id(2)] public string? AssignedToPrincipalId { get; init; }

    public bool Equals(SeatRecord? other) =>
        other != null &&
        SeatId.EqualsOption(other.SeatId) &&
        Date == other.Date &&
        AssignedToPrincipalId.EqualsOption(other.AssignedToPrincipalId);

    public override int GetHashCode() => HashCode.Combine(SeatId, Date, AssignedToPrincipalId);

    public static IValidator<SeatRecord> Validator { get; } = new Validator<SeatRecord>()
        .RuleFor(x => x.SeatId).NotEmpty()
        .RuleFor(x => x.Date).ValidDateTime()
        .Build();
}

[GenerateSerializer]
[Alias("TicketShare.sdk.ProposeSeatChange")]
public sealed record ProposeSeatChange
{
    [Id(0)] public string SeatId { get; init; } = null!;
    [Id(1)] public DateTime Date { get; init; } = DateTime.Now;
    [Id(2)] public string AssignedToPrincipalId { get; init; } = null!;
    [Id(3)] public SeatChangeConfirmed? Confirm { get; init; }

    public bool Equals(ProposeSeatChange? other) =>
        other != null &&
        SeatId.EqualsOption(other.SeatId) &&
        Date == other.Date &&
        AssignedToPrincipalId.EqualsOption(other.AssignedToPrincipalId) &&
        ((Confirm == null && other.Confirm == null) || (Confirm == other.Confirm));

    public override int GetHashCode() => HashCode.Combine(SeatId, Date, AssignedToPrincipalId);

    public static IValidator<ProposeSeatChange> Validator { get; } = new Validator<ProposeSeatChange>()
        .RuleFor(x => x.SeatId).NotEmpty()
        .RuleFor(x => x.Date).ValidDateTime()
        .RuleFor(x => x.AssignedToPrincipalId).NotEmpty()
        .RuleFor(x => x.Confirm).ValidateOption(SeatChangeConfirmed.Validator)
        .Build();
}

public sealed record SeatChangeConfirmed
{
    public DateTime Date { get; init; }
    public bool Confirm { get; init; }
    public string ByPrincipalId { get; init; } = null!;

    public bool Equals(SeatChangeConfirmed? other) =>
        other != null &&
        Date == other.Date &&
        Confirm == other.Confirm &&
        ByPrincipalId.EqualsOption(other.ByPrincipalId);

    public override int GetHashCode() => HashCode.Combine(Date, Confirm, ByPrincipalId);

    public static IValidator<SeatChangeConfirmed> Validator { get; } = new Validator<SeatChangeConfirmed>()
        .RuleFor(x => x.Date).ValidDateTime()
        .RuleFor(x => x.ByPrincipalId).NotEmpty()
        .Build();
}

public static class SeatRecordExtensions
{
    public static Option Validate(this SeatRecord subject) => SeatRecord.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this SeatRecord subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
