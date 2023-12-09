using Toolbox.Extensions;

namespace Toolbox.Tools;

public class ValidEnum<T, TProperty> : ValidatorBase<T, TProperty> where TProperty : struct, Enum
{
    public ValidEnum(IPropertyRule<T, TProperty> rule, string errorMessage)
        : base(rule, errorMessage, x => x.IsEnumValid())
    {
    }
}

public static class ValidEnumExtensions
{
    public static Rule<T, TProperty> ValidEnum<T, TProperty>(this Rule<T, TProperty> rule, string errorMessage = "valid enum is required")
        where TProperty : struct, Enum
    {
        rule.PropertyRule.Validators.Add(new ValidEnum<T, TProperty>(rule.PropertyRule, errorMessage));
        return rule;
    }
}
