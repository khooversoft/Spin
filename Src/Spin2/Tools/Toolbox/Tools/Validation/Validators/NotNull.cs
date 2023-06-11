using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public class NotNull<T, TProperty> : IValidator<TProperty>
{
    private readonly IPropertyRule<T, TProperty> _rule;
    private readonly string _errorMessage;

    public NotNull(IPropertyRule<T, TProperty> rule, string errorMessage)
    {
        _rule = rule.NotNull();
        _errorMessage = errorMessage.NotEmpty();
    }

    public Option<IValidateResult> Validate(TProperty subject)
    {
        return subject switch
        {
            not null => Option<IValidateResult>.None,
            null => _rule.CreateError(_errorMessage),
        };
    }
}
