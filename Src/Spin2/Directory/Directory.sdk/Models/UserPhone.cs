using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Directory.sdk.Models;

public record UserPhone
{
    public string Type { get; init; } = null!;
    public string Number { get; init; } = null!;
}

public static class UserPhoneValidator
{
    public static Validator<UserPhone> Validator { get; } = new Validator<UserPhone>()
        .RuleFor(x => x.Type).NotEmpty()
        .RuleFor(x => x.Number).NotEmpty()
        .Build();

    public static bool IsValid(this UserPhone subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .IsValid(location);
}