using System.Linq.Expressions;
using Toolbox.Types;

namespace Toolbox.Tools;

public interface IValidator<T>
{
    Option<IValidatorResult> Validate(T subject);
}

public class Validator<T> : IValidator<T>
{
    private readonly IList<IPropertyRuleBase<T>> _rules = new List<IPropertyRuleBase<T>>();

    public Rule<T, T> RuleForObject(Func<T, T> func)
    {
        var propertyRule = new PropertyRule<T, T>
        {
            Name = "subject",
            GetValue = func,
        };

        _rules.Add(propertyRule);

        return new Rule<T, T>
        {
            Validator = this,
            PropertyRule = propertyRule
        };
    }

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

    public Rule<T, TProperty> RuleForEach<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> expression)
    {
        MemberExpression expressionBody = (MemberExpression)expression.Body;
        var propertyFunc = expression.Compile();

        var propertyRule = new PropertyCollectionRule<T, TProperty>
        {
            Name = expressionBody.Member.Name,
            GetValue = propertyFunc,
        };

        _rules.Add(propertyRule);

        return new Rule<T, TProperty>
        {
            Validator = this,
            PropertyRule = propertyRule,
        };
    }

    public Option<IValidatorResult> Validate(T subject)
    {
        var result = new ValidatorResult
        {
            Errors = _rules
            .SelectMany(x => x.Validate(subject) switch
            {
                var o when o.HasValue => o.Return() switch
                {
                    ValidatorError v => new[] { v },
                    ValidatorResult v => v.GetErrors(),

                    var v => throw new InvalidOperationException($"Invalid IValidateResult class, type={v.GetType().FullName}"),
                },

                _ => Array.Empty<ValidatorError>(),

            })
            .ToArray()
        };

        return result.Errors switch
        {
            { Count: 0 } => new Option<IValidatorResult>(result, StatusCode.OK, result.ToString()),
            var v => new Option<IValidatorResult>(result, StatusCode.BadRequest, result.ToString())
        };
    }
}


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
