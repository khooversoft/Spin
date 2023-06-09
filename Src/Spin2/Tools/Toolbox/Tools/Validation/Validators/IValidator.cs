using Toolbox.Types.Maybe;

namespace Toolbox.Tools.Validation.Validators;

public interface IValidator<T>
{
    Option<IValidateResult> Validate(T subject);
}
