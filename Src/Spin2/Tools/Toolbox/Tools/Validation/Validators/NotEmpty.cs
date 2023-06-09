using Toolbox.Extensions;
using Toolbox.Types.Maybe;

namespace Toolbox.Tools.Validation.Validators;

public class NotEmpty<T> : IValidator<string>
{
    private readonly IPropertyRule<T, string> _rule;
    private readonly string _errorMessage;

    public NotEmpty(IPropertyRule<T, string> rule, string errorMessage)
    {
        _rule = rule.NotNull();
        _errorMessage = errorMessage.NotEmpty();
    }

    public Option<IValidateResult> Validate(string subject)
    {
        return subject.IsEmpty() switch
        {
            false => Option<IValidateResult>.None,
            true => _rule.CreateError(_errorMessage),
        };
    }
}


public static class NotEmptyExtensions
{
    public static Rule<T, string> NotEmpty<T>(this Rule<T, string> rule, string errorMessage = "is required")
    {
        rule.PropertyRule.Validators.Add(new NotEmpty<T>(rule.PropertyRule, errorMessage));
        return rule;
    }
}