﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public class ValidAccountId<T> : ValidatorBase<T, string>
{
    public ValidAccountId(IPropertyRule<T, string> rule, string errorMessage)
        : base(rule, errorMessage, x => IdPatterns.IsAccountId(x))
    {
    }
}

public static class ValidAccountIdExtensions
{
    public static Rule<T, string> ValidAccountId<T>(this Rule<T, string> rule, string errorMessage = "valid ResourceId is required")
    {
        rule.PropertyRule.Validators.Add(new ValidAccountId<T>(rule.PropertyRule, errorMessage));
        return rule;
    }
}