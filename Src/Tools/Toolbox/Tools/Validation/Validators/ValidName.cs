using Toolbox.Types;

namespace Toolbox.Tools;

public class ValidName<T> : ValidatorBase<T, string>
{
    public ValidName(IPropertyRule<T, string> rule, string errorMessage)
        : base(rule, errorMessage, x => IdPatterns.IsName(x))
    {
    }
}

public class ValidNameOption<T> : ValidatorBase<T, string?>
{
    public ValidNameOption(IPropertyRule<T, string?> rule, string errorMessage)
        : base(rule, errorMessage, x => x == null || IdPatterns.IsName(x))
    {
    }
}


public static class ValidFolderExtensions
{
    public static Rule<T, string> ValidName<T>(this Rule<T, string> rule, string errorMessage = "valid Name is required")
    {
        rule.PropertyRule.Validators.Add(new ValidName<T>(rule.PropertyRule, errorMessage));
        return rule;
    }

    public static Rule<T, string?> ValidNameOption<T>(this Rule<T, string?> rule, string errorMessage = "valid Name is required")
    {
        rule.PropertyRule.Validators.Add(new ValidNameOption<T>(rule.PropertyRule, errorMessage));
        return rule;
    }
}