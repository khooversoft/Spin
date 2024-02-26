using System.Linq.Expressions;

namespace Toolbox.Tools;

public static class ValidatorExtensions
{
    public static Validator<T> Build<T, TProperty>(this Rule<T, TProperty> rule) => rule.Validator;

    public static Rule<T, T> RuleForObject<T, TInput>(this Rule<T, TInput> rule, Func<T, T> func)
    {
        return rule.Validator.RuleForObject(func);
    }

    public static Rule<T, TProperty> RuleFor<T, TInput, TProperty>(this Rule<T, TInput> rule, Expression<Func<T, TProperty>> expression)
    {
        return rule.Validator.RuleFor(expression);
    }

    public static Rule<T, TProperty> RuleForEach<T, TInput, TProperty>(this Rule<T, TInput> rule, Expression<Func<T, IEnumerable<TProperty>>> expression)
    {
        return rule.Validator.RuleForEach(expression);
    }
}
