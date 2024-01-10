using Toolbox.Types;

namespace Toolbox.Tools;


public class ValidateLink<T, TProperty> : IPropertyValidator<T, TProperty>
{
    private readonly IPropertyRule<T, TProperty> _rule;
    private readonly IValidator<TProperty> _validator;
    public ValidateLink(IPropertyRule<T, TProperty> rule, IValidator<TProperty> validator)
    {
        _rule = rule.NotNull();
        _validator = validator.NotNull();
    }

    public Option<IValidatorResult> Validate(T subject, TProperty property)
    {
        return _validator.Validate(property);
    }
}


public class ValidateLinkOption<T, TProperty> : IPropertyValidator<T, TProperty>
{
    private readonly IValidator<TProperty> _validator;
    public ValidateLinkOption(IPropertyRule<T, TProperty?> _, IValidator<TProperty> validator)
    {
        _validator = validator.NotNull();
    }

    public Option<IValidatorResult> Validate(T subject, TProperty? property)
    {
        if (property == null) return Option<IValidatorResult>.None;

        return _validator.Validate(property);
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

    public static Rule<T, TProperty?> ValidateOption<T, TProperty>(this Rule<T, TProperty?> rule, IValidator<TProperty> validator)
    {
        rule.PropertyRule.Validators.Add(new ValidateLinkOption<T, TProperty?>(rule.PropertyRule, validator!));
        return rule;
    }
}
