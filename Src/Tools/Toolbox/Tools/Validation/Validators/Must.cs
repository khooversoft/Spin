using Toolbox.Types;

namespace Toolbox.Tools;


public class MustFunc<T, TProperty> : IPropertyValidator<T, TProperty>
{
    private readonly IPropertyRule<T, TProperty> _rule;
    private readonly Func<TProperty, Option> _check;

    public MustFunc(IPropertyRule<T, TProperty> rule, Func<TProperty, Option> check)
    {
        _rule = rule.NotNull();
        _check = check;
    }

    public Option<IValidatorResult> Validate(T subject, TProperty property)
    {
        var result = _check(property);

        return result.IsOk() switch
        {
            true => result.ToOptionStatus<IValidatorResult>(),
            false => _rule.CreateError(result.Error ?? "<no error>"),
        };
    }
}

public class MustFuncWithSubject<T, TProperty> : IPropertyValidator<T, TProperty>
{
    private readonly IPropertyRule<T, TProperty> _rule;
    private readonly Func<T, TProperty, Option> _check;

    public MustFuncWithSubject(IPropertyRule<T, TProperty> rule, Func<T, TProperty, Option> check)
    {
        _rule = rule.NotNull();
        _check = check;
    }

    public Option<IValidatorResult> Validate(T subject, TProperty property)
    {
        var result = _check(subject, property);

        return result.IsOk() switch
        {
            true => result.ToOptionStatus<IValidatorResult>(),
            false => _rule.CreateError(result.Error ?? "<no error>"),
        };
    }
}


public class MustTest<T, TProperty> : IPropertyValidator<T, TProperty>
{
    private readonly IPropertyRule<T, TProperty> _rule;
    private readonly Func<TProperty, bool> _check;
    private readonly Func<TProperty, string> _getMsg;

    public MustTest(IPropertyRule<T, TProperty> rule, Func<TProperty, bool> check, Func<TProperty, string> getMsg)
    {
        _rule = rule.NotNull();
        _check = check.NotNull();
        _getMsg = getMsg.NotNull();
    }

    public Option<IValidatorResult> Validate(T subject, TProperty property)
    {
        bool result = _check(property);

        return result switch
        {
            true => Option<IValidatorResult>.None,
            false => _rule.CreateError(_getMsg(property)),
        };
    }
}


public static class MustExtensions
{
    public static Rule<T, TProperty> Must<T, TProperty>(this Rule<T, TProperty> rule, Func<TProperty, Option> check)
    {
        rule.PropertyRule.Validators.Add(new MustFunc<T, TProperty>(rule.PropertyRule, check));
        return rule;
    }

    public static Rule<T, TProperty> Must<T, TProperty>(this Rule<T, TProperty> rule, Func<T, TProperty, Option> check)
    {
        rule.PropertyRule.Validators.Add(new MustFuncWithSubject<T, TProperty>(rule.PropertyRule, check));
        return rule;
    }

    public static Rule<T, TProperty> Must<T, TProperty>(this Rule<T, TProperty> rule, Func<TProperty, bool> check, Func<TProperty, string> getMsg)
    {
        getMsg.NotNull();
        rule.PropertyRule.Validators.Add(new MustTest<T, TProperty>(rule.PropertyRule, check, getMsg));
        return rule;
    }
}
