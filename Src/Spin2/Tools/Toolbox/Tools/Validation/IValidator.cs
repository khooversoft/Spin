using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public interface IValidator<TProperty>
{
    Option<IValidateResult> Validate(TProperty subject);
}
