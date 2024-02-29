using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Identity;

public static class IdentityUserTool
{
    public static IValidator<IdentityUser> Validator { get; } = new Validator<IdentityUser>()
        .RuleFor(x => x.Id).NotNull()
        .RuleFor(x => x.UserName).NotNull()
        .RuleFor(x => x.Email).ValidEmailOption()
        .Build();

    public static Option<IValidatorResult> Validate(this IdentityUser subject) => Validator.Validate(subject);
}
