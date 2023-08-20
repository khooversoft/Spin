using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public class ValidObjectId<T> : ValidatorBase<T, string>
{
    public ValidObjectId(IPropertyRule<T, string> rule, string errorMessage)
        : base(rule, errorMessage, x => ObjectId.IsValid(x))
    {
    }
}

public static class ValidObjectIdExtensions
{
    public static Rule<T, string> ValidObjectId<T>(this Rule<T, string> rule, string errorMessage = "valid ObjectId is required")
    {
        rule.PropertyRule.Validators.Add(new ValidObjectId<T>(rule.PropertyRule, errorMessage));
        return rule;
    }
}