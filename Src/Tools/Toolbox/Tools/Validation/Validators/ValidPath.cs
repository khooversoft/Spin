using Toolbox.Types;

namespace Toolbox.Tools;

public class ValidPath<T> : ValidatorBase<T, string>
{
    public ValidPath(IPropertyRule<T, string> rule, string errorMessage)
        : base(rule, errorMessage, x => IdPatterns.IsName(x))
    {
    }
}

public class ValidPathOption<T> : ValidatorBase<T, string?>
{
    public ValidPathOption(IPropertyRule<T, string?> rule, string errorMessage)
        : base(rule, errorMessage, x => IdPatterns.IsPath(x))
    {
    }
}


public static class ValidPathExtensions
{
    public static Rule<T, string> ValidPath<T>(this Rule<T, string> rule, string errorMessage = "valid Name is required")
    {
        rule.PropertyRule.Validators.Add(new ValidPath<T>(rule.PropertyRule, errorMessage));
        return rule;
    }

    public static Rule<T, string?> ValidPathOption<T>(this Rule<T, string?> rule, string errorMessage = "valid Name is required")
    {
        rule.PropertyRule.Validators.Add(new ValidPathOption<T>(rule.PropertyRule, errorMessage));
        return rule;
    }
}