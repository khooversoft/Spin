using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public class ValidPrincipalId<T> : ValidatorBase<T, string>
{
    public ValidPrincipalId(IPropertyRule<T, string> rule, string errorMessage)
        : base(rule, errorMessage, x => IdPatterns.IsPrincipalId(x))
    {
    }
}


public static class ValidOwnerIdExtensions
{
    public static Rule<T, string> ValidPrincipalId<T>(this Rule<T, string> rule, string errorMessage = "valid Principal id is required")
    {
        rule.PropertyRule.Validators.Add(new ValidPrincipalId<T>(rule.PropertyRule, errorMessage));
        return rule;
    }
}
