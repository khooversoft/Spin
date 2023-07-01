using SpinCluster.sdk.Actors.User;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.User;

[GenerateSerializer, Immutable]
public record UserPhone
{
    [Id(0)] public string Type { get; init; } = null!;
    [Id(1)] public string Number { get; init; } = null!;
}

public static class UserPhoneValidator
{
    public static Validator<UserPhone> Validator { get; } = new Validator<UserPhone>()
        .RuleFor(x => x.Type).NotEmpty()
        .RuleFor(x => x.Number).NotEmpty()
        .Build();

    public static bool IsValid(this UserPhone subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .IsValid(location);
}