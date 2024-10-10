using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

[GenerateSerializer]
public sealed record ChangeLog : IEquatable<ChangeLog>
{
    [Id(0)] public DateTime Date { get; init; } = DateTime.UtcNow;
    [Id(1)] public string ChangedByPrincipalId { get; init; } = null!;
    [Id(2)] public string Description { get; init; } = null!;
    [Id(3)] public string? PropertyName { get; init; }
    [Id(4)] public string? OldValue { get; init; }
    [Id(5)] public string? NewValue { get; init; }

    public bool Equals(ChangeLog? other) =>
        other != null &&
        Date == other.Date &&
        ChangedByPrincipalId.Equals(other.ChangedByPrincipalId) &&
        Description.Equals(other.Description) &&
        PropertyName.EqualsIgnoreCase(other.PropertyName) &&
        OldValue.EqualsIgnoreCase(other.OldValue) &&
        NewValue.EqualsIgnoreCase(other.NewValue);

    public override int GetHashCode() => HashCode.Combine(Date, ChangedByPrincipalId, Description);

    public static IValidator<ChangeLog> Validator { get; } = new Validator<ChangeLog>()
        .RuleFor(x => x.Date).ValidDateTime()
        .RuleFor(x => x.Description).NotEmpty()
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