using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public class ValidLeaseId<T> : ValidatorBase<T, string>
{
    public ValidLeaseId(IPropertyRule<T, string> rule, string errorMessage)
        : base(rule, errorMessage, x => IdPatterns.IsLeaseId(x))
    {
    }
}

public static class ValidLeaseIdExtensions
{
    public static Rule<T, string> ValidLeaseId<T>(this Rule<T, string> rule, string errorMessage = "valid Lease ID is required")
    {
        rule.PropertyRule.Validators.Add(new ValidLeaseId<T>(rule.PropertyRule, errorMessage));
        return rule;
    }
}
