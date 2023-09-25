using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Lease;

[GenerateSerializer, Immutable]
public record LeaseData
{
    [Id(0)] public string LeaseKey { get; init; } = null!;
    [Id(1)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(2)] public TimeSpan TimeToLive { get; init; } = TimeSpan.FromSeconds(60);
    [Id(3)] public string? Payload { get; init; }

    public bool IsActive() => (DateTime.UtcNow - CreatedDate) <= TimeToLive;
}


public static class LeaseDataValidator
{
    public static IValidator<LeaseData> Validator { get; } = new Validator<LeaseData>()
        .RuleFor(x => x.LeaseKey).ValidName()
        .RuleFor(x => x.CreatedDate).ValidDateTime()
        .RuleFor(x => x.TimeToLive).Must(x => x.Seconds > 0, _ => "Invalid time to live")
        .Build();

    public static Option Validate(this LeaseData subject) => Validator.Validate(subject).ToOptionStatus();
}