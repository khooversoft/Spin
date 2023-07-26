using Microsoft.Extensions.Logging;
using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public interface IPropertyValidator<TProperty>
{
    Option<IValidatorResult> Validate(TProperty subject);
}