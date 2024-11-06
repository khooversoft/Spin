using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public sealed record ChangeLog : IEquatable<ChangeLog>
{
    public DateTime Date { get; init; } = DateTime.UtcNow;
    public string ChangedByPrincipalId { get; init; } = null!;
    public string SeatId { get; init; } = null!;
    public string? PropertyName { get; init; }
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }

    public bool Equals(ChangeLog? other) =>
        other != null &&
        Date == other.Date &&
        ChangedByPrincipalId == other.ChangedByPrincipalId &&
        SeatId == other.SeatId &&
        PropertyName.EqualsIgnoreCase(other.PropertyName) &&
        OldValue.EqualsIgnoreCase(other.OldValue) &&
        NewValue.EqualsIgnoreCase(other.NewValue);

    public override int GetHashCode() => HashCode.Combine(Date, ChangedByPrincipalId, SeatId);

    public static IValidator<ChangeLog> Validator { get; } = new Validator<ChangeLog>()
        .RuleFor(x => x.Date).ValidDateTime()
        .RuleFor(x => x.SeatId).NotEmpty()
        .RuleFor(x => x.ChangedByPrincipalId).NotEmpty()
        .Build();
}


public static class ChangeLogExtensions
{
    public static Option Validate(this ChangeLog subject) => ChangeLog.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this ChangeLog subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}