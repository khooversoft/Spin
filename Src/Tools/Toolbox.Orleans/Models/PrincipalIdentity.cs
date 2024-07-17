using System.Security.Principal;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans;


[GenerateSerializer]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "ORLEANS0010:Add missing [Alias]", Justification = "<Pending>")]
public sealed record PrincipalIdentity : IIdentity
{
    [Id(0)] public string PrincipalId { get; set; } = null!;
    [Id(1)] public string UserName { get; set; } = null!;
    [Id(2)] public string Email { get; set; } = null!;
    [Id(3)] public bool EmailConfirmed { get; set; }
    [Id(4)] public string PasswordHash { get; set; } = null!;
    [Id(5)] public string NormalizedUserName { get; set; } = null!;
    [Id(6)] public string AuthenticationType { get; set; } = null!;
    [Id(7)] public bool IsAuthenticated { get; set; }
    [Id(8)] public string Name { get; set; } = null!;
    [Id(9)] public string LoginProvider { get; set; } = null!;
    [Id(10)] public string ProviderKey { get; set; } = null!;
    [Id(11)] public string? ProviderDisplayName { get; set; }

    public static IValidator<PrincipalIdentity> Validator { get; } = new Validator<PrincipalIdentity>()
        .RuleFor(x => x.PrincipalId).NotEmpty()
        .RuleFor(x => x.UserName).NotEmpty()
        .RuleFor(x => x.Email).ValidEmail()
        .Build();

    public static IGraphSchema<PrincipalIdentity> Schema { get; } = new GraphSchemaBuilder<PrincipalIdentity>()
        .Node(x => x.PrincipalId, x => IdentityTool.ToUserKey(x))
        .Select(x => x.PrincipalId, x => GraphTool.SelectNodeCommand(IdentityTool.ToUserKey(x), "entity"))
        .Tag(x => x.Email, x => x.IsNotEmpty() ? $"email={x}" : "-email")
        .Index(x => x.UserName, x => IdentityTool.ToUserNameIndex(x))
        .Index(x => x.Email, x => IdentityTool.ToEmailIndex(x))
        .Index(x => x.LoginProvider, x => x.ProviderKey, (x, y) => (x, y) switch
        {
            (string v1, string v2) => IdentityTool.ToLoginIndex(v1, v2),
            _ => null,
        })
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
}
