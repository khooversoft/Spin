using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace LoanContract.sdk.Models;

public sealed record LoanAccountDetail
{
    public string ContractId { get; init; } = null!;
    public string OwnerId { get; init; } = null!;
    public string Name { get; init; } = null!;
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    public IReadOnlyList<AccessBlock> AccessRights { get; init; } = Array.Empty<AccessBlock>();
    public IReadOnlyList<RoleAccessBlock> RoleRights { get; init; } = Array.Empty<RoleAccessBlock>();

    public bool Equals(LoanAccountDetail? obj) => obj is LoanAccountDetail document &&
        ContractId == document.ContractId &&
        OwnerId == document.OwnerId &&
        Name == document.Name &&
        CreatedDate == document.CreatedDate &&
        AccessRights.SequenceEqual(document.AccessRights) &&
        RoleRights.SequenceEqual(document.RoleRights);

    public override int GetHashCode() => HashCode.Combine(ContractId, OwnerId, Name, CreatedDate);

    public static IValidator<LoanAccountDetail> Validator { get; } = new Validator<LoanAccountDetail>()
        .RuleFor(x => x.ContractId).ValidResourceId(ResourceType.DomainOwned, SpinConstants.Schema.Contract)
        .RuleFor(x => x.OwnerId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.CreatedDate).ValidDateTime()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.AccessRights).NotNull()
        .RuleFor(x => x.RoleRights).NotNull()
        .Build();
}


public static class LoanAccountDetailExtensions
{
    public static Option Validate(this LoanAccountDetail subject) => LoanAccountDetail.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this LoanAccountDetail subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}