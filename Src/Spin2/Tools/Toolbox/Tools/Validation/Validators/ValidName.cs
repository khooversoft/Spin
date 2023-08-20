using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public class ValidName<T> : ValidatorBase<T, string>
{
    public ValidName(IPropertyRule<T, string> rule, string errorMessage)
        : base(rule, errorMessage, x => NameId.IsValid(x))
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
}