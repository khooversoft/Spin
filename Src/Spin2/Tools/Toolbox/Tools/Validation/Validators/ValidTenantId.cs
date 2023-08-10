using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public class ValidTenantId<T> : ValidatorBase<T, string>
{
    public ValidTenantId(IPropertyRule<T, string> rule, string errorMessage)
        : base(rule, errorMessage, x => ObjectId.IsValid(x))
    {
    }
}

public static class ValidTenantIdExtensions
{
    public static Rule<T, string> ValidTenantId<T>(this Rule<T, string> rule, string errorMessage = "valid TenantId is required")
    {
        rule.PropertyRule.Validators.Add(new ValidTenantId<T>(rule.PropertyRule, errorMessage));
        return rule;
    }
}