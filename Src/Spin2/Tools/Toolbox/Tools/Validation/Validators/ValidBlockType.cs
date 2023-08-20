using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Block;

public class ValidBlockType<T> : ValidatorBase<T, string>
{
    public ValidBlockType(IPropertyRule<T, string> rule, string errorMessage)
        : base(rule, errorMessage, x => IdPatterns.IsBlockType(x))
    {
    }
}

public static class ValidFolderExtensions
{
    public static Rule<T, string> ValidBlockType<T>(this Rule<T, string> rule, string errorMessage = "valid folder is required")
    {
        rule.PropertyRule.Validators.Add(new ValidBlockType<T>(rule.PropertyRule, errorMessage));
        return rule;
    }
}