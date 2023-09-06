using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public class ValidResourceId<T> : ValidatorBase<T, string>
{
    public ValidResourceId(IPropertyRule<T, string> rule, ResourceType type, string? schema, string errorMessage)
        : base(rule, errorMessage, x => ResourceId.IsValid(x, type, schema))
    {
    }
}

public static class ValidResourceIdExtensions
{
    public static Rule<T, string> ValidResourceId<T>(
        this Rule<T, string> rule,
        ResourceType type, 
        string? schema = null, 
        string errorMessage = "valid ResourceId is required"
        )
    {
        rule.PropertyRule.Validators.Add(new ValidResourceId<T>(rule.PropertyRule, type, schema, errorMessage));
        return rule;
    }
}