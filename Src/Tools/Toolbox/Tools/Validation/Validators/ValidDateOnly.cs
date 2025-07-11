using Toolbox.Extensions;

namespace Toolbox.Tools;

public class ValidDateOnly<T> : ValidatorBase<T, DateOnly>
{
    public ValidDateOnly(IPropertyRule<T, DateOnly> rule, string errorMessage)
        : base(rule, errorMessage, ValidateSubject)
    {
    }

    private static bool ValidateSubject(DateOnly subject) => subject.IsDateOnlyValid();
}

public class ValidDateOnlyOption<T> : ValidatorBase<T, DateOnly?>
{
    public ValidDateOnlyOption(IPropertyRule<T, DateOnly?> rule, string errorMessage)
        : base(rule, errorMessage, ValidateSubject)
    {
    }

    private static bool ValidateSubject(DateOnly? subject) => subject.IsDateOnlyValid();
}

public static class ValidDateOnlyTool
{
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
}
