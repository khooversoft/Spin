using System.Text.Json.Serialization;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public record PrincipalIdentity
{
    public const string NodeType = "principal";
    public const string NodeReferenceType = "principal-ref";
    public const string NameIdentifierClaimType = "nameidentifier";
    public const string UserNameClaimType = "username";
    public const string EmailClaimType = "email";

    public PrincipalIdentity(string principalId, string userName, string email, bool emailConfirmed = false)
    {
        PrincipalId = principalId.NotEmpty();
        NameIdentifier = principalId.NotEmpty();
        UserName = userName.NotEmpty();
        Email = email.NotEmpty();
        EmailConfirmed = emailConfirmed;

        NodeKey = NodeTool.CreateKey(PrincipalId, NodeType);
    }

    [JsonConstructor]
    public PrincipalIdentity(string principalId, string nameIdentifier, string userName, string email, bool emailConfirmed)
    {
        PrincipalId = principalId.NotEmpty();
        NameIdentifier = nameIdentifier.NotEmpty();
        UserName = userName.NotEmpty();
        Email = email.NotEmpty();
        EmailConfirmed = emailConfirmed;

        NodeKey = NodeTool.CreateKey(PrincipalId, NodeType);
    }

    public string NodeKey { get; }
    public string PrincipalId { get; init; }              // Id of the principal
    public string NameIdentifier { get; init; }           // Identity provider's PK for the user
    public string UserName { get; init; }
    public string Email { get; init; }
    public bool EmailConfirmed { get; init; }

    public static IValidator<PrincipalIdentity> Validator { get; } = new Validator<PrincipalIdentity>()
        .RuleFor(x => x.NodeKey).NotEmpty()
        .RuleFor(x => x.PrincipalId).NotEmpty()
        .RuleFor(x => x.NameIdentifier).NotEmpty()
        .RuleFor(x => x.UserName).NotEmpty()
        .RuleFor(x => x.Email).ValidEmail()
        .Build();
}

public static class PrincipalIdentityTool
{
    public static Option Validate(this PrincipalIdentity subject) => PrincipalIdentity.Validator.Validate(subject).ToOptionStatus();

    public static PrincipalIdentity Update(this PrincipalIdentity subject, string? nameIdentifier, string? userName, string? email, bool? emailConfirmed)
    {
        return subject with
        {
            NameIdentifier = nameIdentifier.ToNullIfEmpty() ?? subject.NameIdentifier,
            UserName = userName.ToNullIfEmpty() ?? subject.UserName,
            Email = email.ToNullIfEmpty() ?? subject.Email,
            EmailConfirmed = emailConfirmed ?? subject.EmailConfirmed,
        };
    }

    public static string CreateNameIdentifierNodeKey(this PrincipalIdentity subject) => NodeTool.CreateKey(subject.NameIdentifier, PrincipalIdentity.NameIdentifierClaimType);
    public static string CreateUserNameNodeKey(this PrincipalIdentity subject) => NodeTool.CreateKey(subject.UserName, PrincipalIdentity.UserNameClaimType);
    public static string CreateEmailNodeKey(this PrincipalIdentity subject) => NodeTool.CreateKey(subject.Email, PrincipalIdentity.EmailClaimType);
    public static bool IsNodeType(string nodeKey) => NodeTool.ParseKey(nodeKey).NodeType.EqualsIgnoreCase(PrincipalIdentity.NodeType);
}
