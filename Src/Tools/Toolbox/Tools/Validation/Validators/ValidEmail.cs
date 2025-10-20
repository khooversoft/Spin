namespace Toolbox.Tools;


public class ValidEmail<T> : ValidatorBase<T, string>
{
    public ValidEmail(IPropertyRule<T, string> rule, string errorMessage)
        : base(rule, errorMessage, StandardValidation.IsEmail)
    {
    }
}

public class ValidEmailOption<T> : ValidatorBase<T, string?>
{
    public ValidEmailOption(IPropertyRule<T, string?> rule, string errorMessage)
        : base(rule, errorMessage, x => x == null || StandardValidation.IsEmail(x))
    {
    }
}


public static class ValidEmailTool
{
    public static Rule<T, string> ValidEmail<T>(this Rule<T, string> rule, string errorMessage = "valid Email is required")
    {
        rule.PropertyRule.Validators.Add(new ValidEmail<T>(rule.PropertyRule, errorMessage));
        return rule;
    }

    public static Rule<T, string?> ValidEmailOption<T>(this Rule<T, string?> rule, string errorMessage = "valid Email is required")
    {
        rule.PropertyRule.Validators.Add(new ValidEmailOption<T>(rule.PropertyRule, errorMessage));
        return rule;
    }
}

