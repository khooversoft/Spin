using Toolbox.Types;

namespace Toolbox.Tools;

public abstract class ValidatorBase<T, TProperty> : IPropertyValidator<T, TProperty>
{
    private readonly IPropertyRule<T, TProperty> _rule;
    private readonly string _errorMessage;
    private readonly Func<TProperty, bool> _isValid;

    public ValidatorBase(IPropertyRule<T, TProperty> rule, string errorMessage, Func<TProperty, bool> isValid)
    {
        _rule = rule.NotNull();
        _errorMessage = errorMessage.NotEmpty();
        _isValid = isValid.NotNull();
    }

    public Option<IValidatorResult> Validate(T subject, TProperty property)
    {
        return _isValid(property) switch
        {
            true => Option<IValidatorResult>.None,
            false => _rule.CreateError(_errorMessage),
        };
    }
}