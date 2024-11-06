using Toolbox.Tools;

public class ValidDateOnly<T> : ValidatorBase<T, DateOnly>
{
    public ValidDateOnly(IPropertyRule<T, DateOnly> rule, string errorMessage)
        : base(rule, errorMessage, ValidateSubject)
    {
    }

    private static bool ValidateSubject(DateOnly subject) => ValidDateOnlyTool.IsValidDateTime(subject);
}

public class ValidDateOnlyOption<T> : ValidatorBase<T, DateOnly?>
{
    public ValidDateOnlyOption(IPropertyRule<T, DateOnly?> rule, string errorMessage)
        : base(rule, errorMessage, ValidateSubject)
    {
    }

    private static bool ValidateSubject(DateOnly? subject) => ValidDateOnlyTool.IsValidDateTime(subject);
}

public static class ValidDateOnlyTool
{
    private static readonly DateOnly _minRange = new DateOnly(1900, 1, 1);
    private static readonly DateOnly _maxRange = new DateOnly(2199, 12, 31);

    public static Rule<T, DateOnly> ValidDateOnly<T>(this Rule<T, DateOnly> rule, string errorMessage = "valid DateOnly is required, range 1900-01-01 to 2100-01-01")
    {
        rule.PropertyRule.Validators.Add(new ValidDateOnly<T>(rule.PropertyRule, errorMessage));
        return rule;
    }

    public static Rule<T, DateOnly?> ValidDateOnlyOption<T>(this Rule<T, DateOnly?> rule, string errorMessage = "valid DateOnly is required, range 1900-01-01 to 2100-01-01")
    {
        rule.PropertyRule.Validators.Add(new ValidDateOnlyOption<T>(rule.PropertyRule, errorMessage));
        return rule;
    }

    public static bool IsValidDateTime(DateOnly? date) => date switch
    {
        DateOnly v when v >= _minRange && v <= _maxRange => true,
        _ => false,
    };
}
