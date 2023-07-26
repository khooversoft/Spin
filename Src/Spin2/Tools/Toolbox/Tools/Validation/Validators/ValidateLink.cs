using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public class ValidateLink<T, TProperty> : IPropertyValidator<TProperty>
{
    private readonly IPropertyRule<T, TProperty> _rule;
    private readonly IValidator<TProperty> _validator;
    public ValidateLink(IPropertyRule<T, TProperty> rule, IValidator<TProperty> validator)
    {
        _rule = rule.NotNull();
        _validator = validator.NotNull();
    }

    public Option<IValidatorResult> Validate(TProperty subject)
    {
        return _validator.Validate(subject);
    }
}


public static class ValidateLinkExtensions
{
    public static Rule<T, TProperty> Validate<T, TProperty>(this Rule<T, TProperty> rule, IValidator<TProperty> validator)
    {
        rule.NotNull();
        rule.PropertyRule.Validators.Add(new ValidateLink<T, TProperty>(rule.PropertyRule, validator));
        return rule;
    }
}