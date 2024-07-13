using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public readonly struct Property
{
    public string Key { get; init; }
    public string? Value { get; init; }

    public static IValidator<Property> Validator { get; } = new Validator<Property>()
        .RuleFor(x => x.Key).NotEmpty()
        .Build();
}

public static class PropertyExtensions
{
    public static Option Validate(this Property subject) => Property.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this Property subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}