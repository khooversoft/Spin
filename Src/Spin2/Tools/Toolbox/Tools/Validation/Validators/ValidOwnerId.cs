using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public class ValidOwnerId<T> : IPropertyValidator<string>
{
    private readonly IPropertyRule<T, string> _rule;
    private readonly string _errorMessage;

    public ValidOwnerId(IPropertyRule<T, string> rule, string errorMessage)
    {
        _rule = rule.NotNull();
        _errorMessage = errorMessage.NotEmpty();
    }

    public Option<IValidateResult> Validate(string subject)
    {
        return OwnerId.IsValid(subject) switch
        {
            true => Option<IValidateResult>.None,
            false => _rule.CreateError(_errorMessage),
        };
    }
}


public static class ValidOwnerIdExtensions
{
    public static Rule<T, string> ValidOwnerId<T>(this Rule<T, string> rule, string errorMessage = "valid owner id is required")
    {
        rule.PropertyRule.Validators.Add(new ValidOwnerId<T>(rule.PropertyRule, errorMessage));
        return rule;
    }
}