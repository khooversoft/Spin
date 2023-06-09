using Toolbox.Types.Maybe;

namespace Toolbox.Tools.Validation.Validators;

public class ValidateLink<T, TProperty> : IValidator<T>
{
    private readonly IPropertyRule<T, TProperty> _rule;
    private readonly Validator<TProperty> _validator;
    public ValidateLink(IPropertyRule<T, TProperty> rule, Validator<TProperty> validator)
    {
        _rule = rule.NotNull();
        _validator = validator.NotNull();
    }

    public Option<IValidateResult> Validate(T subject)
    {
        var property = _rule.GetValue(subject);
        return _validator.Validate(property);
    }
}


public static class ValidateLinkExtensions
{
    public static Rule<T, TProperty> Validate<T, TProperty>(this Rule<T, TProperty> rule, Validator<TProperty> validator)
    {
        rule.PropertyRule.Validators.Add(new ValidateLink<T, TProperty>(rule.PropertyRule, validator));
        return rule;
    }
}