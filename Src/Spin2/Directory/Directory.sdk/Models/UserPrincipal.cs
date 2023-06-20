using Azure;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Directory.sdk.Models;

public record UserPrincipal
{
    private const string _version = nameof(UserPrincipal) + "-v1";

    public string UserId { get; init; } = null!;
    public string Version { get; init; } = _version;
    public string GlobalPrincipleId { get; init; } = Guid.NewGuid().ToString();

    public string DisplayName { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string? Email { get; init; }
    public IReadOnlyList<UserPhone> Phone { get; init; } = Array.Empty<UserPhone>();
    public IReadOnlyList<UserAddress> Addresses { get; init; } = Array.Empty<UserAddress>();

    public DataObjectCollection DataObjects { get; init; } = new DataObjectCollection();
    public bool AccountEnabled { get; init; } = false;
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    public DateTime? ActiveDate { get; init; }
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
        .RuleFor(x => x.DataObjects).Validate(DataObjectCollectionValidator.Validator)
        .Build();

    public static ValidatorResult Validate(this UserPrincipal subject) => Validator.Validate(subject);

    public static UserPrincipal Verify(this UserPrincipal subject) => subject.Action(x => x.Validate().ThrowOnError());
}
