using Toolbox.Types.Maybe;

namespace Toolbox.Tools.Validation.Validators;

public class ValidateLink<T, TProperty> : IValidator<TProperty>
{
    private readonly IPropertyRule<T, TProperty> _rule;
    private readonly Validator<TProperty> _validator;
    public ValidateLink(IPropertyRule<T, TProperty> rule, Validator<TProperty> validator)
    {
        _rule = rule.NotNull();
        _validator = validator.NotNull();
    }

    public Option<IValidateResult> Validate(TProperty subject)
    {
        return _validator.Validate(subject);
    }
}


public static class ValidateLinkExtensions
{
    public static Rule<T, TProperty> Validate<T, TProperty>(this Rule<T, TProperty> rule, Validator<TProperty> validator)
    {
        rule.NotNull();
        rule.PropertyRule.Validators.Add(new ValidateLink<T, TProperty>(rule.PropertyRule, validator));
        return rule;
    }
}