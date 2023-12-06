using SpinCluster.abstraction;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace LoanContract.sdk.Models;

public sealed record LoanAccountDetail
{
    public string ContractId { get; init; } = null!;
    public string OwnerId { get; init; } = null!;
    public string Name { get; init; } = null!;
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    public IReadOnlyList<AccessBlock> Access { get; init; } = new List<AccessBlock>();
    public IReadOnlyList<RoleAccessBlock> RoleAccess { get; init; } = Array.Empty<RoleAccessBlock>();

    public bool Equals(LoanAccountDetail? obj) => obj is LoanAccountDetail document &&
        ContractId == document.ContractId &&
        OwnerId == document.OwnerId &&
        Name == document.Name &&
        CreatedDate == document.CreatedDate &&
        Access.SequenceEqual(document.Access);

    public override int GetHashCode() => HashCode.Combine(ContractId, OwnerId, Name, CreatedDate);

    public static AccessBlock[] CreatePaymentAccess(string principalId) => [
        AccessBlock.Create<LoanLedgerItem>(BlockGrant.Write, principalId),
        AccessBlock.Create<LoanDetail>(BlockGrant.Read, principalId),
    ];

    public static IValidator<LoanAccountDetail> Validator { get; } = new Validator<LoanAccountDetail>()
        .RuleFor(x => x.ContractId).ValidResourceId(ResourceType.DomainOwned, SpinConstants.Schema.Contract)
        .RuleFor(x => x.OwnerId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.CreatedDate).ValidDateTime()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleForEach(x => x.Access).Validate(AccessBlock.Validator)
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