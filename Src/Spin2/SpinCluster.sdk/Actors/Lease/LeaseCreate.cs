using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Lease;

[GenerateSerializer, Immutable]
public class LeaseCreate
{
    public LeaseCreate() { }

    public LeaseCreate(string leaseKey) => LeaseKey = leaseKey;

    [Id(0)] public string LeaseKey { get; init; } = null!;
    [Id(1)] public string? Payload { get; init; }
    [Id(2)] public TimeSpan TimeToLive { get; init; } = TimeSpan.FromSeconds(30);
}

public static class LeaseDataCreateValidator
{
    public static IValidator<LeaseCreate> Validator { get; } = new Validator<LeaseCreate>()
        .RuleFor(x => x.LeaseKey).NotEmpty()
        .RuleFor(x => x.TimeToLive).Must(x => x.Seconds > 0, _ => "Invalid time to live")
        .Build();

    public static Option Validate(this LeaseCreate subject) => Validator.Validate(subject).ToOptionStatus();
}
