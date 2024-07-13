using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public record ChangeLog
{
    public DateTime Date { get; init; } = DateTime.UtcNow;
    public string ChangedByPrincipalId { get; init; } = null!;
    public string Description { get; init; } = null!;
    public string? PropertyName { get; init; }
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }

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