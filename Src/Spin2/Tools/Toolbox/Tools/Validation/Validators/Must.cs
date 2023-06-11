using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public class MustFunc<T, TProperty> : IValidator<TProperty>
{
    private readonly IPropertyRule<T, TProperty> _rule;
    private readonly Func<TProperty, Option<string>> _check;

    public MustFunc(IPropertyRule<T, TProperty> rule, Func<TProperty, Option<string>> check)
    {
        _rule = rule.NotNull();
        _check = check.NotNull();
    }

    public Option<IValidateResult> Validate(TProperty subject)
    {
        var result = _check(subject);

        return result.HasValue switch
        {
            false => result.ToOption<IValidateResult>(),
            true => _rule.CreateError(result.Return()),
        };
    }
}


public class MustTest<T, TProperty> : IValidator<TProperty>
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

    public Option<IValidateResult> Validate(TProperty subject)
    {
        bool result = _check(subject);

        return result switch
        {
            true => Option<IValidateResult>.None,
            false => _rule.CreateError(_getMsg(subject)),
        };
    }
}


public static class MustExtensions
{
    public static Rule<T, TProperty> Must<T, TProperty>(this Rule<T, TProperty> rule, Func<TProperty, Option<string>> check)
    {
        rule.PropertyRule.Validators.Add(new MustFunc<T, TProperty>(rule.PropertyRule, check));
        return rule;
    }

    public static Rule<T, TProperty> Must<T, TProperty>(this Rule<T, TProperty> rule, Func<TProperty, bool> check, Func<TProperty, string> getMsg)
    {
        getMsg.NotNull();
        rule.PropertyRule.Validators.Add(new MustTest<T, TProperty>(rule.PropertyRule, check, getMsg));
        return rule;
    }
}
