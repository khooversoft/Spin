using System.Linq.Expressions;
using Toolbox.Types;

namespace Toolbox.Tools;

public interface IValidator<in T>
{
    Option<IValidatorResult> Validate(T subject);
}

public class Validator<T> : IValidator<T>
{
    private readonly List<IPropertyRuleBase<T>> _rules = new List<IPropertyRuleBase<T>>();

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
        var errors = new List<IValidatorResult>();

        foreach (var rule in _rules)
        {
            var result = rule.Validate(subject);
            if (!result.HasValue) continue;

            var value = result.Return();
            if (value is ValidatorError ve)
            {
                errors.Add(ve);
            }
            else if (value is ValidatorResult vr)
            {
                var nested = vr.GetErrors();
                if (nested.Count != 0)
                {
                    foreach (var e in nested) errors.Add(e);
                }
            }
            else
            {
                throw new InvalidOperationException($"Invalid IValidateResult class, type={value.GetType().FullName}");
            }
        }

        var final = new ValidatorResult
        {
            Errors = errors.ToArray(),
        };

        return final.Errors switch
        {
            { Count: 0 } => new Option<IValidatorResult>(final, StatusCode.OK, final.ToString()),
            _ => new Option<IValidatorResult>(final, StatusCode.BadRequest, final.ToString()),
        };
    }
}
