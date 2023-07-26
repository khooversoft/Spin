using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public class NotEmpty<T> : IPropertyValidator<string>
{
    private readonly IPropertyRule<T, string> _rule;
    private readonly string _errorMessage;

    public NotEmpty(IPropertyRule<T, string> rule, string errorMessage)
    {
        _rule = rule.NotNull();
        _errorMessage = errorMessage.NotEmpty();
    }

    public Option<IValidatorResult> Validate(string subject)
    {
        return subject.IsEmpty() switch
        {
            false => Option<IValidatorResult>.None,
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