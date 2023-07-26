using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public class ValidDateTime<T> : IPropertyValidator<DateTime>
{
    private static readonly DateTime _minRange = new DateTime(1900, 1, 1);
    private static readonly DateTime _maxRange = new DateTime(2199, 12, 31);
    private readonly IPropertyRule<T, DateTime> _rule;
    private readonly string _errorMessage;

    public ValidDateTime(IPropertyRule<T, DateTime> rule, string errorMessage)
    {
        _rule = rule.NotNull();
        _errorMessage = errorMessage.NotEmpty();
    }

    public Option<IValidatorResult> Validate(DateTime subject)
    {
        return subject switch
        {
            var v when v >= _minRange && v <= _maxRange => Option<IValidatorResult>.None,
            _ => _rule.CreateError(_errorMessage),
        };
    }
}


public static class ValidDateTimeExtensions
{
    public static Rule<T, DateTime> ValidDateTime<T>(this Rule<T, DateTime> rule, string errorMessage = "valid DateTime is required, range 1900-01-01 to 2100-01-01")
    {
        rule.PropertyRule.Validators.Add(new ValidDateTime<T>(rule.PropertyRule, errorMessage));
        return rule;
    }
}