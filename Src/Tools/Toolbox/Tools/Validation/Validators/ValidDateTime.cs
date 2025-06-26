using Toolbox.Extensions;

namespace Toolbox.Tools;


public class ValidDateTime<T> : ValidatorBase<T, DateTime>
{
    public ValidDateTime(IPropertyRule<T, DateTime> rule, string errorMessage)
        : base(rule, errorMessage, ValidateSubject)
    {
    }

    private static bool ValidateSubject(DateTime subject) => subject.IsDateTimeValid();
}

public class ValidDateTimeOption<T> : ValidatorBase<T, DateTime?>
{
    public ValidDateTimeOption(IPropertyRule<T, DateTime?> rule, string errorMessage)
        : base(rule, errorMessage, ValidateSubject)
    {
    }

    private static bool ValidateSubject(DateTime? subject) => subject == null || subject.IsDateTimeValid();
}

public static class ValidDateTimeTool
{
    public static Rule<T, DateTime> ValidDateTime<T>(this Rule<T, DateTime> rule, string errorMessage = "Valid DateTime is required, range 1900-01-01 to 2100-01-01")
    {
        rule.PropertyRule.Validators.Add(new ValidDateTime<T>(rule.PropertyRule, errorMessage));
        return rule;
    }

    public static Rule<T, DateTime?> ValidDateTimeOption<T>(this Rule<T, DateTime?> rule, string errorMessage = "Valid DateTime is required, range 1900-01-01 to 2100-01-01")
    {
        rule.PropertyRule.Validators.Add(new ValidDateTimeOption<T>(rule.PropertyRule, errorMessage));
        return rule;
    }
}
