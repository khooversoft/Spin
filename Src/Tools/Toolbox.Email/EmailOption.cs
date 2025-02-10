using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Email;

public record EmailOption
{
    public string Email { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string Name { get; init; } = null!;

    public static IValidator<EmailOption> Validator { get; } = new Validator<EmailOption>()
        .RuleFor(x => x.Email).NotEmpty()
        .RuleFor(x => x.Password).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .Build();
}


public static class EmailOptionExtensions
{
    public static Option Validate(this EmailOption subject) => EmailOption.Validator.Validate(subject).ToOptionStatus();
}
