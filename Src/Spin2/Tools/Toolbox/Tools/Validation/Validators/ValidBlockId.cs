using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public class ValidBlockId<T> : ValidatorBase<T, string>
{
    public ValidBlockId(IPropertyRule<T, string> rule, string errorMessage)
        : base(rule, errorMessage, x => IdPatterns.IsBlockId(x))
    {
    }
}

public static class ValidBlockIdExtensions
{
    public static Rule<T, string> ValidBlockId<T>(this Rule<T, string> rule, string errorMessage = "valid BlockId is required")
    {
        rule.PropertyRule.Validators.Add(new ValidBlockId<T>(rule.PropertyRule, errorMessage));
        return rule;
    }
}