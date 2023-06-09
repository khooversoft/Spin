using System.Linq.Expressions;
using Toolbox.Types.Maybe;

namespace Toolbox.Tools.Validation;

public class Validator<T>
{
    private readonly IList<IPropertyRuleBase<T>> _rules = new List<IPropertyRuleBase<T>>();

    public Rule<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        MemberExpression expressionBody = (MemberExpression)expression.Body;
        var propertyFunc = expression.Compile();

        var propertyRule = new PropertyRule<T, TProperty>
        {
            Name = expressionBody.Member.Name,
            GetValue = propertyFunc,
        };

        _rules.Add(propertyRule);

        return new Rule<T, TProperty>
        {
            Validator = this,
            PropertyRule = propertyRule
        };
    }

    public ValidatorResult<T> Validate(T subject)
    {
        return new ValidatorResult<T>
        {
            Subject = subject,

            Errors = _rules
                .SelectMany(x => x.Validate(subject) switch
                {
                    var o when o.HasValue => o.Return() switch
                    {
                        ValidatorError v => new[] { v },
                        ValidatorResult<T> v => v.Errors,

                        var v => throw new InvalidOperationException($"Invalid IValidateResult class, type={v.GetType().FullName}"),
                    },

                    _ => Array.Empty<ValidatorError>(),

                })
                .ToArray(),
        };
    }
}


public static class ValidatorExtensions
{
    public static Validator<T> Build<T, TProperty>(this Rule<T, TProperty> rule) => rule.Validator;

    public static Rule<T, TProperty> RuleFor<T, TInput, TProperty>(this Rule<T, TInput> rule, Expression<Func<T, TProperty>> expression)
    {
        return rule.Validator.RuleFor(expression);
    }
}
