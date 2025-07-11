namespace Toolbox.Tools;


public class ValidTimeSpan<T> : ValidatorBase<T, TimeSpan>
{
    public ValidTimeSpan(IPropertyRule<T, TimeSpan> rule, string errorMessage)
        : base(rule, errorMessage, ValidateSubject)
    {
    }

    private static bool ValidateSubject(TimeSpan subject) => subject.IsValidTimeSpan();
}

public class ValidTimeSpanOption<T> : ValidatorBase<T, TimeSpan?>
{
    public ValidTimeSpanOption(IPropertyRule<T, TimeSpan?> rule, string errorMessage)
        : base(rule, errorMessage, ValidateSubject)
    {
    }

    private static bool ValidateSubject(TimeSpan? subject) => subject == null || subject.IsValidTimeSpan();
}

public static class ValidTimeSpanTool
{
    public static Rule<T, TimeSpan> ValidTimeSpan<T>(this Rule<T, TimeSpan> rule, string errorMessage = "Valid TimeSpan is required")
    {
        rule.PropertyRule.Validators.Add(new ValidTimeSpan<T>(rule.PropertyRule, errorMessage));
        return rule;
    }

    public static Rule<T, TimeSpan?> ValidTimeSpanOption<T>(this Rule<T, TimeSpan?> rule, string errorMessage = "Valid TimeSpan")
    {
        rule.PropertyRule.Validators.Add(new ValidTimeSpanOption<T>(rule.PropertyRule, errorMessage));
        return rule;
    }

    public static bool IsValidTimeSpan(this TimeSpan subject) => subject.TotalMilliseconds > 0;

    public static bool IsValidTimeSpan(this TimeSpan? subject)
    {
        if (subject == null) return false;
        return subject.Value.IsValidTimeSpan();
    }
}
