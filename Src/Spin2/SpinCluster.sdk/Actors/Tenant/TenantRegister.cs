using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.User;
using Toolbox.Tools.Validation;

namespace SpinCluster.sdk.Actors.Tenant;

[GenerateSerializer, Immutable]
public record TenantRegister
{
    [Id(0)] public string TenantId { get; init; } = null!;
    [Id(1)] public string GlobalPrincipleId { get; init; } = Guid.NewGuid().ToString();
    [Id(2)] public string TenantName { get; init; } = null!;
    [Id(4)] public string Contact { get; init; } = null!;
    [Id(5)] public string Email { get; init; } = null!;
    [Id(6)] public IReadOnlyList<UserPhone> Phone { get; init; } = Array.Empty<UserPhone>();
    [Id(7)] public IReadOnlyList<UserAddress> Addresses { get; init; } = Array.Empty<UserAddress>();

    [Id(8)] public IReadOnlyList<DataObject> DataObjects { get; init; } = Array.Empty<DataObject>();
    [Id(9)] public bool AccountEnabled { get; init; } = false;
    [Id(10)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(11)] public DateTime? ActiveDate { get; init; }
}


public static class TenantRegisterValidator
{
    public static Validator<TenantRegister> Validator { get; } = new Validator<TenantRegister>()
        .RuleFor(x => x.TenantId).NotEmpty()
        .RuleFor(x => x.GlobalPrincipleId).NotEmpty()
        .RuleFor(x => x.Contact).NotEmpty()
        .RuleFor(x => x.Email).NotEmpty()
        .RuleFor(x => x.Phone).NotNull().Must(x => x.Count > 0, _ => "A phone is required")
        .RuleFor(x => x.Addresses).NotNull().Must(x => x.Count > 0, _ => "A address is required")
        .RuleFor(x => x.DataObjects).NotNull()
        .Build();

    public static ValidatorResult Validate(this TenantRegister subject) => Validator.Validate(subject);
}