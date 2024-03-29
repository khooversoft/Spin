﻿namespace Toolbox.Tools;

public class NotNull<T, TProperty> : ValidatorBase<T, TProperty>
{
    public NotNull(IPropertyRule<T, TProperty> rule, string errorMessage)
        : base(rule, errorMessage, x => x != null)
    {
    }
}

