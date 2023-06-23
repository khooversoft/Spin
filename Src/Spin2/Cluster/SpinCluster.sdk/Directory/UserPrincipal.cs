using SpinCluster.sdk.ActorBase;
using SpinCluster.sdk.Directory;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Directory;

[GenerateSerializer, Immutable]
public record UserPrincipal
{
    private const string _version = nameof(UserPrincipal) + "-v1";

    [Id(0)] public string UserId { get; init; } = null!;
    [Id(1)] public string Version { get; init; } = _version;
    [Id(2)] public string GlobalPrincipleId { get; init; } = Guid.NewGuid().ToString();

    [Id(3)] public string DisplayName { get; init; } = null!;
    [Id(4)] public string FirstName { get; init; } = null!;
    [Id(5)] public string LastName { get; init; } = null!;
    [Id(6)] public string? Email { get; init; }
    [Id(7)] public IReadOnlyList<UserPhone> Phone { get; init; } = Array.Empty<UserPhone>();
    [Id(8)] public IReadOnlyList<UserAddress> Addresses { get; init; } = Array.Empty<UserAddress>();

    [Id(9)] public IReadOnlyList<DataObject> DataObjects { get; init; } = Array.Empty<DataObject>();
    [Id(10)] public bool AccountEnabled { get; init; } = false;
    [Id(11)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(12)] public DateTime? ActiveDate { get; init; }
}


public static class UserPrincipalValidator
{
    public static Validator<UserPrincipal> Validator { get; } = new Validator<UserPrincipal>()
        .RuleFor(x => x.UserId).NotEmpty().Must(x => ObjectId.IsValid(x), x => $"{x} is not a valid ObjectId")
        .RuleFor(x => x.Version).NotEmpty()
        .RuleFor(x => x.GlobalPrincipleId).NotEmpty()
        .RuleFor(x => x.DisplayName).NotEmpty()
        .RuleFor(x => x.FirstName).NotEmpty()
        .RuleFor(x => x.LastName).NotEmpty()
        .RuleFor(x => x.Phone).NotNull()
        .RuleForEach(x => x.Phone).Validate(UserPhoneValidator.Validator)
        .RuleFor(x => x.Addresses).NotNull()
        .RuleForEach(x => x.Addresses).Validate(UserAddressValidator.Validator)
        .RuleForEach(x => x.DataObjects).NotNull()
        .RuleForEach(x => x.DataObjects).Validate(DataObjectValidator.Validator)
        .Build();

    public static ValidatorResult Validate(this UserPrincipal subject) => Validator.Validate(subject);

    public static UserPrincipal Verify(this UserPrincipal subject) => subject.Action(x => x.Validate().ThrowOnError());
}
