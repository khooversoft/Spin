using SpinCluster.abstraction;
using SpinCluster.sdk.Application;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace LoanContract.sdk.Contract;

public record LoanSchedulerContext
{
    public string SchedulerId { get; init; } = null!;
    public string SmartcId { get; init; } = null!;
    public string ContractId { get; init; } = null!;
    public string PrincipalId { get; init; } = null!;
    public string SourceId { get; init; } = null!;

    public static IValidator<LoanSchedulerContext> Validator { get; } = new Validator<LoanSchedulerContext>()
        .RuleFor(x => x.SchedulerId).ValidResourceId(ResourceType.System, SpinConstants.Schema.Scheduler)
        .RuleFor(x => x.SmartcId).ValidResourceId(ResourceType.DomainOwned, SpinConstants.Schema.Smartc)
        .RuleFor(x => x.ContractId).ValidResourceId(ResourceType.DomainOwned, SpinConstants.Schema.Contract)
        .RuleFor(x => x.PrincipalId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.SourceId).ValidName()
        .Build();
}


public static class LoanSchedulerContextExtensions
{
    public static Option Validate(this LoanSchedulerContext subject) => LoanSchedulerContext.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this LoanSchedulerContext subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
