using Toolbox.Tools;

public class ValidDateTime<T> : ValidatorBase<T, DateTime>
{
    public ValidDateTime(IPropertyRule<T, DateTime> rule, string errorMessage)
        : base(rule, errorMessage, ValidateSubject)
    {
    }

    private static bool ValidateSubject(DateTime subject) => ValidDateTimeTool.IsValidDateTime(subject);
}

public static class ValidDateTimeTool
{
    private static readonly DateTime _minRange = new DateTime(1900, 1, 1);
    private static readonly DateTime _maxRange = new DateTime(2199, 12, 31);

    public static Rule<T, DateTime> ValidDateTime<T>(this Rule<T, DateTime> rule, string errorMessage = "valid DateTime is required, range 1900-01-01 to 2100-01-01")
    {
        rule.PropertyRule.Validators.Add(new ValidDateTime<T>(rule.PropertyRule, errorMessage));
        return rule;
    }

    public static bool IsValidDateTime(DateTime? date) => date switch
    {
        DateTime v when v >= _minRange && v <= _maxRange => true,
        _ => false,
    };
}
