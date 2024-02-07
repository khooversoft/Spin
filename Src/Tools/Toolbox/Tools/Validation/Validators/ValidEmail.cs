using System.Net.Mail;
using Toolbox.Extensions;

namespace Toolbox.Tools;

public class ValidEmail<T> : ValidatorBase<T, string>
{
    public ValidEmail(IPropertyRule<T, string> rule, string errorMessage)
        : base(rule, errorMessage, ValidEmailTool.IsValidEmail)
    {
    }
}

public class ValidEmailOption<T> : ValidatorBase<T, string?>
{
    public ValidEmailOption(IPropertyRule<T, string?> rule, string errorMessage)
        : base(rule, errorMessage, ValidEmailTool.IsValidEmail)
    {
    }
}


public static class ValidEmailTool
{
    public static Rule<T, string> ValidEmail<T>(this Rule<T, string> rule, string errorMessage = "valid Email is required")
    {
        rule.PropertyRule.Validators.Add(new ValidEmail<T>(rule.PropertyRule, errorMessage));
        return rule;
    }

    public static Rule<T, string?> ValidEmailOption<T>(this Rule<T, string?> rule, string errorMessage = "valid Email is required")
    {
        rule.PropertyRule.Validators.Add(new ValidEmailOption<T>(rule.PropertyRule, errorMessage));
        return rule;
    }

    public static bool IsValidEmail(string? email)
    {
        if (email.IsEmpty()) return false;

        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

