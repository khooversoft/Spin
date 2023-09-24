using SoftBank.sdk.Application;
using Toolbox.Block;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SoftBank.sdk.Models;

[GenerateSerializer, Immutable]
public sealed record SbAccountDetail
{
    [Id(0)] public string AccountId { get; init; } = null!;
    [Id(1)] public string OwnerId { get; init; } = null!;
    [Id(2)] public string Name { get; init; } = null!;
    [Id(3)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(4)] public IReadOnlyList<AccessBlock> AccessRights { get; init; } = Array.Empty<AccessBlock>();
    [Id(5)] public IReadOnlyList<RoleAccessBlock> RoleRights { get; init; } = Array.Empty<RoleAccessBlock>();

    public bool Equals(SbAccountDetail? obj) => obj is SbAccountDetail document &&
        AccountId == document.AccountId &&
        OwnerId == document.OwnerId &&
        Name == document.Name &&
        CreatedDate == document.CreatedDate &&
        AccessRights.SequenceEqual(document.AccessRights) &&
        RoleRights.SequenceEqual(document.RoleRights);

    public override int GetHashCode() => HashCode.Combine(AccountId, OwnerId, Name, CreatedDate);

    public static IValidator<SbAccountDetail> Validator { get; } = new Validator<SbAccountDetail>()
        .RuleFor(x => x.AccountId).ValidResourceId(ResourceType.DomainOwned, SoftBankConstants.Schema.SoftBankSchema)
        .RuleFor(x => x.OwnerId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.CreatedDate).ValidDateTime()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.AccessRights).NotNull()
        .RuleFor(x => x.RoleRights).NotNull()
        .Build();
}


public static class AccountDetailValidator
{
    public static Option Validate(this SbAccountDetail subject) => SbAccountDetail.Validator.Validate(subject).ToOptionStatus();
}