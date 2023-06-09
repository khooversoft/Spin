using Azure;
using Toolbox.Tools.Validation;
using Toolbox.Tools.Validation.Validators;
using Toolbox.Types;
using Toolbox.Tools;
using Toolbox.Extensions;

namespace Directory.sdk.Models;

public record IdentityEntry
{
    private const string _version = nameof(IdentityEntry) + "-v1";

    public string DirectoryId { get; init; } = null!;
    public string Subject { get; init; } = null!;
    public string Version { get; init; } = _version;
    public ETag? ETag { get; init; }
    public byte[] PublicKey { get; init; } = null!;
    public byte[]? PrivateKey { get; init; }
    public IList<string> Properties { get; init; } = new List<string>();
}


public static class IdentityEntryValidator
{
    public static Validator<IdentityEntry> Validator = new Validator<IdentityEntry>()
        .RuleFor(x => x.DirectoryId).NotEmpty().Must(x => ObjectId.IsValid(x), x => $"{x} is not a valid ObjectId")
        .RuleFor(x => x.Subject).NotEmpty()
        .RuleFor(x => x.Version).NotEmpty()
        .RuleFor(x => x.PublicKey).NotNull()
        .RuleFor(x => x.Properties).NotNull()
        .Build();

    public static ValidatorResult Validate(this IdentityEntry subject) => Validator.Validate(subject);

    public static IdentityEntry Verify(this IdentityEntry subject) => subject.Action(x => x.Validate().ThrowOnError());
}
