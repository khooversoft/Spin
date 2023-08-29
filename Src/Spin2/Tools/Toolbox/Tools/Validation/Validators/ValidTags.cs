using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public class ValidTags<T> : ValidatorBase<T, string?>
{
    public ValidTags(IPropertyRule<T, string?> rule, string errorMessage)
        : base(rule, errorMessage, x => x.IsEmpty() || IdPatterns.IsTags(x))
    {
    }
}

public static class ValidTagsExtensions
{
    public static Rule<T, string?> ValidTags<T>(this Rule<T, string?> rule, string errorMessage = "valid tag is required")
    {
        rule.PropertyRule.Validators.Add(new ValidTags<T>(rule.PropertyRule, errorMessage));
        return rule;
    }
}