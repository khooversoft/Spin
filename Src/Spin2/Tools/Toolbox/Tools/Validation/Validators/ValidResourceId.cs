using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools;

public class ValidResourceId<T> : ValidatorBase<T, string>
{
    public ValidResourceId(IPropertyRule<T, string> rule, ResourceType type, string? schema, string errorMessage)
        : base(rule, errorMessage, x => x.IsEmpty() || ResourceId.IsValid(x, type, schema))
    {
    }
}

public class ValidResourceIdOption<T> : ValidatorBase<T, string?>
{
    public ValidResourceIdOption(IPropertyRule<T, string?> rule, ResourceType type, string? schema, string errorMessage)
        : base(rule, errorMessage, x => x == null || ResourceId.IsValid(x, type, schema))
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
        rule.NotNull();
        rule.PropertyRule.Validators.Add(new ValidResourceId<T>(rule.PropertyRule, type, schema, errorMessage));
        return rule;
    }

    public static Rule<T, string?> ValidResourceIdOption<T>(
        this Rule<T, string?> rule,
        ResourceType type,
        string? schema = null,
        string errorMessage = "valid ResourceId is required"
        )
    {
        rule.PropertyRule.Validators.Add(new ValidResourceIdOption<T>(rule.PropertyRule, type, schema, errorMessage));
        return rule;
    }
}