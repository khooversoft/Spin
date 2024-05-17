using System.Security.Principal;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans;

[GenerateSerializer]
public sealed record PrincipalIdentity : IIdentity
{
    [Id(0)] public string Id { get; set; } = null!;
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
        .RuleFor(x => x.Id).NotNull()
        .RuleFor(x => x.UserName).NotNull()
        .RuleFor(x => x.Email).ValidEmail()
        .Build();

    //public static IGraphSchema<PrincipalIdentity> GraphSchema { get; } = new GraphSchema<PrincipalIdentity>()
    //    .Node(x => PrincipalIdentityTool.ToUserKey(x.Id))
    //    .Index("userName", x => PrincipalIdentityTool.ToUserNameIndex(x.UserName))
    //    .Index("email", x => PrincipalIdentityTool.ToEmailIndex(x.Email))
    //    .Tags("emailTag", x => PrincipalIdentityTool.ToEmailTag(x.Email))
    //    .Index("logonProvider", x => PrincipalIdentityTool.ToLogonProvider(x.LoginProvider, x.ProviderKey))
    //    .Build();
}


public static class PrincipalIdentityTool
{
    //private static FrozenSet<string> _ns = new string[] { "user", "userName", "userEmail", "logonProvider" }.ToFrozenSet();
    //public static string RemoveNs(string key) => _ns.Aggregate(key, (a, x) => key.StartsWith(x) ? key[0..(x.Length - 1)] : key);
    public static Option Validate(this PrincipalIdentity subject) => PrincipalIdentity.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this PrincipalIdentity subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    //public static string ToUserKey(string id) => $"user:{id.NotEmpty().ToLower()}";
    //public static string ToUserNameIndex(string userName) => $"userName:{userName.NotEmpty().ToLower()}";
    //public static string ToEmailIndex(string userName) => $"userEmail:{userName.NotEmpty().ToLower()}";
    //public static string ToEmailTag(string email) => $"email={email.NotEmpty().ToLower()}";
    //public static string ToLogonProvider(string provider, string providerKey) => $"logonProvider:{provider.NotEmpty().ToLower() + "/" + providerKey.NotEmpty().ToLower()}";

    //public static Option<string> GenerateAddNodeCommands(this PrincipalIdentity subject)
    //{
    //    if (!subject.Validate(out var r)) return r.ToOptionStatus<string>();

    //    var commands = subject.GraphSchema.CreateAddCommands(subject);
    //    return commands;
    //}
}
