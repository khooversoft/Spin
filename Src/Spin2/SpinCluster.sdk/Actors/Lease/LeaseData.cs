using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Lease;

[GenerateSerializer, Immutable]
public record LeaseData
{
    [Id(0)] public string LeaseId { get; init; } = Guid.NewGuid().ToString();
    [Id(1)] public string AccountId { get; init; } = null!;
    [Id(2)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(3)] public TimeSpan TimeToLive { get; init; } = TimeSpan.FromSeconds(60);
    [Id(4)] public string LeaseKey { get; init; } = null!;
    [Id(5)] public string? Payload { get; init; }
}


public static class LeaseDataValidator
{
    public static IValidator<LeaseData> Validator { get; } = new Validator<LeaseData>()
        .RuleFor(x => x.LeaseId).ValidLeaseId()
        .RuleFor(x => x.AccountId).ValidAccountId()
        .RuleFor(x => x.CreatedDate).ValidDateTime()
        .RuleFor(x => x.TimeToLive).Must(x => x.Seconds > 0, _ => "Invalid time to live")
        .RuleFor(x => x.LeaseKey).ValidName()
        .Build();

    public static Option Validate(this LeaseData subject) => Validator.Validate(subject).ToOptionStatus();

    public static bool IsLeaseValid(this LeaseData data) => data != null && DateTime.UtcNow < data.CreatedDate + data.TimeToLive;
}