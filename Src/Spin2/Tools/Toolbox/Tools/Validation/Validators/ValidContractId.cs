using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public class ValidContractId<T> : ValidatorBase<T, string>
{
    public ValidContractId(IPropertyRule<T, string> rule, string errorMessage)
        : base(rule, errorMessage, x => IdPatterns.IsContractId(x))
    {
    }
}

public static class ValidBlockIdExtensions
{
    public static Rule<T, string> ValidContractId<T>(this Rule<T, string> rule, string errorMessage = "valid ContractId is required")
    {
        rule.PropertyRule.Validators.Add(new ValidContractId<T>(rule.PropertyRule, errorMessage));
        return rule;
    }
}