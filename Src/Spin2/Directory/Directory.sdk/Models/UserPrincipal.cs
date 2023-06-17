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

    public string DirectoryId { get; init; } = null!;
    public string Subject { get; init; } = null!;
    public string Version { get; init; } = _version;
    public ETag? ETag { get; init; }
    public byte[] PublicKey { get; init; } = null!;
    public byte[]? PrivateKey { get; init; }
    public DataObjectCollection DataObjects { get; init; } = new DataObjectCollection();

    public string DisplayName { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string PrincipleName { get; init; } = null!;
    public string PrincipleId { get; init; } = Guid.NewGuid().ToString();
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    public DateTime? ActiveDate { get; init; }
    public bool AccountEnabled { get; init; } = false;
}


public static class UserPrincipalExtensions
{
    public static Validator<UserPrincipal> Validator { get; } = new Validator<UserPrincipal>()
        .RuleFor(x => x.DirectoryId).NotEmpty().Must(x => ObjectId.IsValid(x), x => $"{x} is not a valid ObjectId")
        .RuleFor(x => x.Subject).NotEmpty()
        .RuleFor(x => x.Version).NotEmpty()
        .RuleFor(x => x.PublicKey).NotNull()
        .RuleFor(x => x.DataObjects).Validate(DataObjectCollectionValidator.Validator)
        .Build();

    public static ValidatorResult Validate(this UserPrincipal subject) => Validator.Validate(subject);

    public static UserPrincipal Verify(this UserPrincipal subject) => subject.Action(x => x.Validate().ThrowOnError());
}
