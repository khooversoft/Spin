using Toolbox.Types;

namespace Toolbox.Tools;

public interface IPropertyValidator<TProperty>
{
    Option<IValidatorResult> Validate(TProperty subject);
}