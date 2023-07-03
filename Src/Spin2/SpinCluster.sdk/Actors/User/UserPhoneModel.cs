using SpinCluster.sdk.Actors.Key;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.User;

[GenerateSerializer, Immutable]
public record UserPhoneModel
{
    [Id(0)] public string Type { get; init; } = null!;
    [Id(1)] public string Number { get; init; } = null!;
}

public static class UserPhoneModelValidator
{
    public static IValidator<UserPhoneModel> Validator { get; } = new Validator<UserPhoneModel>()
        .RuleFor(x => x.Type).NotEmpty()
        .RuleFor(x => x.Number).NotEmpty()
        .Build();

    public static ValidatorResult Validate(this UserPhoneModel subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .LogResult(location);
}