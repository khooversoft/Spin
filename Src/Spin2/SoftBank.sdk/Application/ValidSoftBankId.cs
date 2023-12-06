using Toolbox.Tools;

namespace SoftBank.sdk.Application;

public class ValidSoftBankId<T> : ValidatorBase<T, string>
{
    public ValidSoftBankId(IPropertyRule<T, string> rule, string errorMessage)
        : base(rule, errorMessage, x => IdSoftbank.IsSoftBankId(x))
    {
    }
}

public static class ValidSoftBankIdExtensions
{
    public static Rule<T, string> ValidSoftBankId<T>(this Rule<T, string> rule, string errorMessage = "valid ContractId is required")
    {
        rule.PropertyRule.Validators.Add(new ValidSoftBankId<T>(rule.PropertyRule, errorMessage));
        return rule;
    }
}