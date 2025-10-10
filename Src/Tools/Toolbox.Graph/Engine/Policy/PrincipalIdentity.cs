using System.Text.Json.Serialization;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public record PrincipalIdentity
{
    public PrincipalIdentity(string nameIdentifier, string userName, string email, bool emailConfirmed = false)
    {
        PrincipalId = $"user:{RandomTool.GenerateRandomSequence()}";
        NameIdentifier = nameIdentifier.NotEmpty();
        UserName = userName.NotEmpty();
        Email = email.NotEmpty();
        EmailConfirmed = emailConfirmed;
    }

    [JsonConstructor]
    public PrincipalIdentity(string principalId, string nameIdentifier, string userName, string email, bool emailConfirmed)
    {
        PrincipalId = principalId.NotEmpty();
        NameIdentifier = nameIdentifier.NotEmpty();
        UserName = userName.NotEmpty();
        Email = email.NotEmpty();
        EmailConfirmed = emailConfirmed;
    }

    // Id - GetById e.g. 'user:9b0d4bed' - IdentityTool.GeneratedNodeKey() - this is the ID used for security
    public string PrincipalId { get; }
    public string NameIdentifier { get; }         // Identity provider's PK for the user
    public string UserName { get; }
    public string Email { get; }
    public bool EmailConfirmed { get; }

    public static IValidator<PrincipalIdentity> Validator { get; } = new Validator<PrincipalIdentity>()
        .RuleFor(x => x.PrincipalId).NotEmpty()
        .RuleFor(x => x.NameIdentifier).NotEmpty()
        .RuleFor(x => x.UserName).NotEmpty()
        .RuleFor(x => x.Email).ValidEmail()
        .Build();
}

public static class PrincipalIdentityTool
{
    public static Option Validate(this PrincipalIdentity subject) => PrincipalIdentity.Validator.Validate(subject).ToOptionStatus();
}
