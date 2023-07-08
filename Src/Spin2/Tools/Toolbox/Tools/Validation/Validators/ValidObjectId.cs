using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public class ValidObjectId<T> : IPropertyValidator<string>
{
    private readonly IPropertyRule<T, string> _rule;
    private readonly string _errorMessage;

    public ValidObjectId(IPropertyRule<T, string> rule, string errorMessage)
    {
        _rule = rule.NotNull();
        _errorMessage = errorMessage.NotEmpty();
    }

    public Option<IValidateResult> Validate(string subject)
    {
        return ObjectId.IsValid(subject) switch
        {
            true => Option<IValidateResult>.None,
            false => _rule.CreateError(_errorMessage),
        };
    }
}


public static class ValidObjectIdExtensions
{
    public static Rule<T, string> ValidObjectId<T>(this Rule<T, string> rule, string errorMessage = "valid ObjectId is required")
    {
        rule.PropertyRule.Validators.Add(new NotEmpty<T>(rule.PropertyRule, errorMessage));
        return rule;
    }
}