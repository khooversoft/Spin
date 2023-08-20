using Toolbox.Extensions;

namespace Toolbox.Tools.Validation;

public class NotEmpty<T> : ValidatorBase<T, string>
{
    public NotEmpty(IPropertyRule<T, string> rule, string errorMessage)
        : base(rule, errorMessage, x => x.IsNotEmpty())
    {
    }
}


public static class NotEmptyExtensions
{
    public static Rule<T, string> NotEmpty<T>(this Rule<T, string> rule, string errorMessage = "is required")
    {
        rule.PropertyRule.Validators.Add(new NotEmpty<T>(rule.PropertyRule, errorMessage));
        return rule;
    }
}