using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions;

public static class IdentityTool
{
    public const string NodeTag = "principalIdentity";
    public const string NodeKeyPrefix = "user:";
    public static string ToNodeKey(string principalId) => $"{NodeKeyPrefix}{principalId.NotEmpty().ToLower()}";
    public static string RemoveNodeKeyPrefix(string subject) => subject.NotEmpty().StartsWith(NodeKeyPrefix) switch
    {
        false => subject,
        true => subject[NodeKeyPrefix.Length..],
    };

    public static string ConstructEmailTag(string value) => $"email={value.ToLower()}";

    public static string ConstructUserNameTag(string value) => $"userName={value.ToLower()}";

    public static string? ConstructLoginProviderTag(string? loginProvider, string? providerKey)
    {
        return (loginProvider.IsNotEmpty() && providerKey.IsNotEmpty()) switch
        {
            false => null,
            true => $"loginProvider={loginProvider.ToLower()}/{providerKey.ToLower()}",
        };
    }

    public static Option<string> CreateQuery(this PrincipalIdentity user, ScopeContext context)
    {
        if (user.Validate().IsError(out var r)) return r.LogStatus(context, nameof(PrincipalIdentity)).ToOptionStatus<string>();

        string nodeKey = IdentityTool.ToNodeKey(user.PrincipalId);

        string emailTag = IdentityTool.ConstructEmailTag(user.Email);
        string userNameNameTag = IdentityTool.ConstructUserNameTag(user.UserName.ToNullIfEmpty() ?? user.NormalizedUserName);
        string loginProviderTag = IdentityTool.ConstructLoginProviderTag(user.LoginProvider, user.ProviderKey) ?? "-loginProvider";

        var cmd = new NodeCommandBuilder()
            .UseSet()
            .SetNodeKey(nodeKey)
            .AddTag(IdentityTool.NodeTag)
            .AddTag(emailTag)
            .AddTag(userNameNameTag)
            .AddTag(loginProviderTag)
            .AddData("entity", user)
            .AddIndex("email")
            .AddIndex("userName")
            .Action(x =>
            {
                if (user.HasLoginProvider()) x.AddIndex("loginProvider");
            })
            .Build();

        return cmd;
    }

    public static CommandBatchBuilder AddIdentity(this CommandBatchBuilder builder, PrincipalIdentity subject)
    {
        builder.Add((context) => CreateQuery(subject, context));
        return builder;
    }
}
