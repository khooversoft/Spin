using Azure;
using FluentValidation;

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


public class IdentityEntryValidator : AbstractValidator<IdentityEntry>
{
    public static IdentityEntryValidator Default { get; } = new IdentityEntryValidator();

    public IdentityEntryValidator()
    {
        RuleFor(x => x.DirectoryId).NotEmpty();
        RuleFor(x => x.Subject).NotEmpty();
        RuleFor(x => x.Version).NotEmpty();
        RuleFor(x => x.PublicKey).NotNull();
    }
}


public static class IdentityEntryExtensions
{
    public static void Verify(this IdentityEntry subject) => IdentityEntryValidator.Default.ValidateAndThrow(subject);

    public static bool IsVerify(this IdentityEntry subject) => IdentityEntryValidator.Default.Validate(subject).IsValid;

    public static IReadOnlyList<string> GetVerifyErrors(this IdentityEntry subject) => IdentityEntryValidator.Default
        .Validate(subject)
        .Errors
        .Select(x => x.ErrorMessage)
        .ToArray();
}
