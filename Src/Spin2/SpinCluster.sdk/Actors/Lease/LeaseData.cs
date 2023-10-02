using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Lease;

[GenerateSerializer, Immutable]
public record LeaseData
{
    [Id(0)] public string LeaseKey { get; init; } = null!;
    [Id(1)] public string? Reference { get; init; }
    [Id(2)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(3)] public TimeSpan TimeToLive { get; init; } = TimeSpan.FromMinutes(5);
    [Id(4)] public string? Payload { get; init; }
    [Id(5)] public string? Tags { get; init; }

    public bool IsActive() => (DateTime.UtcNow - CreatedDate) <= TimeToLive;

    public static IValidator<LeaseData> Validator { get; } = new Validator<LeaseData>()
        .RuleFor(x => x.LeaseKey).NotEmpty()
        .RuleFor(x => x.CreatedDate).ValidDateTime()
        .RuleFor(x => x.TimeToLive).Must(x => x.TotalSeconds > 0, _ => "Invalid time to live")
        .Build();
}


public static class LeaseDataValidator
{
    public static Option Validate(this LeaseData subject) => LeaseData.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this LeaseData subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}