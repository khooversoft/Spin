using SpinCluster.sdk.Actors.User;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.User;

[GenerateSerializer, Immutable]
public record UserAddress
{
    [Id(0)] public string Type { get; init; } = null!;
    [Id(1)] public string Address1 { get; init; } = null!;
    [Id(2)] public string? Address2 { get; init; }
    [Id(3)] public string City { get; init; } = null!;
    [Id(4)] public string State { get; init; } = null!;
    [Id(5)] public string ZipCode { get; init; } = null!;
    [Id(6)] public string Country { get; init; } = null!;
}


public static class UserAddressValidator
{
    public static Validator<UserAddress> Validator { get; } = new Validator<UserAddress>()
        .RuleFor(x => x.Type).NotEmpty()
        .RuleFor(x => x.Address1).NotEmpty()
        .RuleFor(x => x.City).NotEmpty()
        .RuleFor(x => x.State).NotEmpty()
        .RuleFor(x => x.ZipCode).NotEmpty()
        .RuleFor(x => x.Country).NotEmpty()
        .Build();

    public static bool IsValid(this UserAddress subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .IsValid(location);
}