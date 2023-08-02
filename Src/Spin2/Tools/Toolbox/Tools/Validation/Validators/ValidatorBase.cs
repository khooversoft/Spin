using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public abstract class ValidatorBase<T, TProperty> : IPropertyValidator<TProperty>
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

    public Option<IValidatorResult> Validate(TProperty subject)
    {
        return _isValid(subject) switch
        {
            true => Option<IValidatorResult>.None,
            false => _rule.CreateError(_errorMessage),
        };
    }
}