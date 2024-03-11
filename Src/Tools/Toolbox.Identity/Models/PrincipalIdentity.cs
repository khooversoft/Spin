using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Identity;

public sealed record PrincipalIdentity : IIdentity
{
    public string Id { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool EmailConfirmed { get; set; }
    public string PasswordHash { get; set; } = null!;
    public string NormalizedUserName { get; internal set; } = null!;
    public string AuthenticationType { get; set; } = null!;
    public bool IsAuthenticated { get; set; }
    public string Name { get; set; } = null!;
}


public static class PrincipalIdentityExtensions
{
    public static IValidator<PrincipalIdentity> Validator { get; } = new Validator<PrincipalIdentity>()
        .RuleFor(x => x.Id).NotNull()
        .RuleFor(x => x.UserName).NotNull()
        .RuleFor(x => x.Email).ValidEmail()
        .Build();

    public static Option<IValidatorResult> Validate(this PrincipalIdentity subject) => Validator.Validate(subject);
}