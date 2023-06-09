using Toolbox.Types.Maybe;

namespace Toolbox.Tools.Validation.Validators;

public class NotNull<T, TProperty> : IValidator<T>
{
    private readonly IPropertyRule<T, TProperty> _rule;
    private readonly string _errorMessage;

    public NotNull(IPropertyRule<T, TProperty> rule, string errorMessage)
    {
        _rule = rule.NotNull();
        _errorMessage = errorMessage.NotEmpty();
    }

    public Option<IValidateResult> Validate(T subject)
    {
        return _rule.GetValue(subject) switch
        {
            not null => Option<IValidateResult>.None,
            null => _rule.CreateError(_errorMessage),
        };
    }
}
