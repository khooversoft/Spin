using Toolbox.Block;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SoftBank.sdk.Models;

[GenerateSerializer, Immutable]
public sealed record AccountDetail
{
    [Id(0)] public string DocumentId { get; init; } = null!;
    [Id(1)] public string OwnerId { get; init; } = null!;
    [Id(2)] public string Name { get; init; } = null!;
    [Id(3)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(4)] public IReadOnlyList<BlockAccess> AccessRights { get; init; } = Array.Empty<BlockAccess>();

    public bool Equals(AccountDetail? obj) => obj is AccountDetail document &&
        DocumentId == document.DocumentId &&
        OwnerId == document.OwnerId &&
        Name == document.Name &&
        CreatedDate == document.CreatedDate &&
        AccessRights.SequenceEqual(document.AccessRights);

    public override int GetHashCode() => HashCode.Combine(DocumentId, OwnerId, Name, CreatedDate);
}


public static class AccountDetailValidator
{
    public static IValidator<AccountDetail> Validator { get; } = new Validator<AccountDetail>()
        .RuleFor(x => x.DocumentId).ValidContractId()
        .RuleFor(x => x.OwnerId).ValidPrincipalId()
        .RuleFor(x => x.CreatedDate).ValidDateTime()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.AccessRights).NotNull()
        .Build();

    public static Option Validate(this AccountDetail subject) => Validator.Validate(subject).ToOptionStatus();
}