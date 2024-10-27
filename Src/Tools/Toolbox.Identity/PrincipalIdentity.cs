using System.Security.Principal;
using Orleans;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Identity;

//
// PrincipalIdentity =
//    pk = 'user:{PrincipalId}'
//    index = 'email:{Email}'
//    index = 'logonProvider:{LoginProvider}/{ProviderKey}"


[GenerateSerializer]
public sealed record PrincipalIdentity : IIdentity
{
    // Id - GetById e.g. 'user:user1@domain.com' - ToUserKey()
    [Id(0)] public string PrincipalId { get; set; } = null!;
    [Id(1)] public string UserName { get; set; } = null!;

    [Id(2)] public string Email { get; set; } = null!;
    [Id(3)] public bool EmailConfirmed { get; set; }
    [Id(4)] public string PasswordHash { get; set; } = null!;

    [Id(5)] public string NormalizedUserName { get; set; } = null!;
    [Id(6)] public string AuthenticationType { get; set; } = null!;
    [Id(7)] public bool IsAuthenticated { get; set; }
    [Id(8)] public string Name { get; set; } = null!;

    [Id(9)] public string? LoginProvider { get; set; } = null!;
    [Id(10)] public string? ProviderKey { get; set; } = null!;
    [Id(11)] public string? ProviderDisplayName { get; set; }

    public static IValidator<PrincipalIdentity> Validator { get; } = new Validator<PrincipalIdentity>()
        .RuleFor(x => x.PrincipalId).NotEmpty()
        .RuleFor(x => x.UserName).NotEmpty()
        .RuleFor(x => x.Email).ValidEmail()
        .RuleFor(x => x.NormalizedUserName).NotEmpty()
        .Build();
}


public static class PrincipalIdentityTool
{
    public static Option Validate(this PrincipalIdentity subject) => PrincipalIdentity.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this PrincipalIdentity subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static bool HasLoginProvider(this PrincipalIdentity subject) => subject.NotNull().LoginProvider.IsNotEmpty() && subject.ProviderKey.IsNotEmpty();

}
