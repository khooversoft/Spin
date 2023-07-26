using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public class ValidPrincipalId<T> : IPropertyValidator<string>
{
    private readonly IPropertyRule<T, string> _rule;
    private readonly string _errorMessage;

    public ValidPrincipalId(IPropertyRule<T, string> rule, string errorMessage)
    {
        _rule = rule.NotNull();
        _errorMessage = errorMessage.NotEmpty();
    }

    public Option<IValidatorResult> Validate(string subject)
    {
        return PrincipalId.IsValid(subject) switch
        {
            true => Option<IValidatorResult>.None,
            false => _rule.CreateError(_errorMessage),
        };
    }
}


public static class ValidOwnerIdExtensions
{
    public static Rule<T, string> ValidPrincipalId<T>(this Rule<T, string> rule, string errorMessage = "valid owner id is required")
    {
        rule.PropertyRule.Validators.Add(new ValidPrincipalId<T>(rule.PropertyRule, errorMessage));
        return rule;
    }
}