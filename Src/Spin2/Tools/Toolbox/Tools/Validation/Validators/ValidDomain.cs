using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public class ValidDomain<T> : ValidatorBase<T, string>
{
    public ValidDomain(IPropertyRule<T, string> rule, string errorMessage)
        : base(rule, errorMessage, x => IdPatterns.IsDomain(x))
    {
    }
}


public static class ValidDomainExtensions
{
    public static Rule<T, string> ValidDomain<T>(this Rule<T, string> rule, string errorMessage = "valid domain is required")
    {
        rule.PropertyRule.Validators.Add(new ValidDomain<T>(rule.PropertyRule, errorMessage));
        return rule;
    }
}
