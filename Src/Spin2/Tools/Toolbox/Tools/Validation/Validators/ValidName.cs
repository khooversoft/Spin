using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public class ValidName<T> : IPropertyValidator<string>
{
    private readonly IPropertyRule<T, string> _rule;
    private readonly string _errorMessage;

    public ValidName(IPropertyRule<T, string> rule, string errorMessage)
    {
        _rule = rule.NotNull();
        _errorMessage = errorMessage.NotEmpty();
    }

    public Option<IValidateResult> Validate(string subject)
    {
        return NameId.IsValid(subject) switch
        {
            true => Option<IValidateResult>.None,
            false => _rule.CreateError(_errorMessage),
        };
    }
}


public static class ValidFolderExtensions
{
    public static Rule<T, string> ValidName<T>(this Rule<T, string> rule, string errorMessage = "valid folder is required")
    {
        rule.PropertyRule.Validators.Add(new ValidName<T>(rule.PropertyRule, errorMessage));
        return rule;
    }
}