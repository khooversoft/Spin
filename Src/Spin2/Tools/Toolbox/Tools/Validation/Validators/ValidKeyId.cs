using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public class ValidKeyId<T> : ValidatorBase<T, string>
{
    public ValidKeyId(IPropertyRule<T, string> rule, string errorMessage)
        : base(rule, errorMessage, x => IdPatterns.IsKeyId(x))
    {
    }
}

public static class ValidValidKeyIdExtensions
{
    public static Rule<T, string> ValidKeyId<T>(this Rule<T, string> rule, string errorMessage = "valid KeyId is required")
    {
        rule.PropertyRule.Validators.Add(new ValidKeyId<T>(rule.PropertyRule, errorMessage));
        return rule;
    }
}