using SpinCluster.abstraction;
using Toolbox.Tools;
using Toolbox.Types;

namespace LoanContract.sdk;

public sealed record LoanInterestRequest
{
    public string ContractId { get; init; } = null!;
    public string PrincipalId { get; init; } = null!;
    public DateTime PostedDate { get; init; } = DateTime.UtcNow;
    public string? Tags { get; init; }

    public static IValidator<LoanInterestRequest> Validator { get; } = new Validator<LoanInterestRequest>()
        .RuleFor(x => x.ContractId).ValidResourceId(ResourceType.DomainOwned, SpinConstants.Schema.Contract)
        .RuleFor(x => x.PrincipalId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.PostedDate).ValidDateTime()
        .Build();
}


public static class LoanInterestRequestExtensions
{
    public static Option Validate(this LoanInterestRequest subject) => LoanInterestRequest.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this LoanInterestRequest subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
