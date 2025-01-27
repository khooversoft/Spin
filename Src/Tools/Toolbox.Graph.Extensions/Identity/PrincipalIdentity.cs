using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions;

//
// PrincipalIdentity =
//    pk = 'user:{PrincipalId}'
//    index = 'email:{Email}'
//    index = 'logonProvider:{LoginProvider}/{ProviderKey}"


public sealed record PrincipalIdentity
{
    // Id - GetById e.g. 'user:user1@domain.com' - ToUserKey()
    public string PrincipalId { get; set; } = null!;
    public string UserName { get; set; } = null!;
    // Email is normalized
    public string Email { get; set; } = null!;
    public string NormalizedUserName { get; set; } = null!;
    public string? Name { get; set; }
    public string? LoginProvider { get; set; } = null!;
    public string? ProviderKey { get; set; } = null!;
    public string? ProviderDisplayName { get; set; }

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

    public static bool HasLoginProvider(this PrincipalIdentity subject) => subject.NotNull().LoginProvider.IsNotEmpty() && subject.ProviderKey.IsNotEmpty();
}
