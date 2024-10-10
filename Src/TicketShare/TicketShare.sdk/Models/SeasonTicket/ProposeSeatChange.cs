﻿using Toolbox.Extensions;
using Toolbox.Tools;

namespace TicketShare.sdk;

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
        SeatId.EqualsIgnoreCase(other.SeatId) &&
        Date == other.Date &&
        AssignedToPrincipalId.EqualsIgnoreCase(other.AssignedToPrincipalId) &&
        ((Confirm == null && other.Confirm == null) || (Confirm == other.Confirm));

    public override int GetHashCode() => HashCode.Combine(SeatId, Date, AssignedToPrincipalId);

    public static IValidator<ProposeSeatChange> Validator { get; } = new Validator<ProposeSeatChange>()
        .RuleFor(x => x.SeatId).NotEmpty()
        .RuleFor(x => x.Date).ValidDateTime()
        .RuleFor(x => x.AssignedToPrincipalId).NotEmpty()
        .RuleFor(x => x.Confirm).ValidateOption(SeatChangeConfirmed.Validator)
        .Build();
}
