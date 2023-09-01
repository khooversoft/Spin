using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public class ValidResourceId<T> : ValidatorBase<T, string>
{
    public ValidResourceId(IPropertyRule<T, string> rule, ResourceType type, string? schema, string errorMessage)
        : base(rule, errorMessage, x => isValid(x, type, schema))
    {
    }

    private static bool isValid(string value, ResourceType type, string? schema) => ResourceId.Create(value) switch
    {
        { StatusCode: StatusCode.OK } v => v.Return() switch
        {
            var r when r.Type == type && (schema == null || r.Schema == schema) => true,
            _ => false,
        },

        _ => false,
    };
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