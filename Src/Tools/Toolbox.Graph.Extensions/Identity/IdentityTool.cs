//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph.Extensions;

//public static class IdentityTool
//{
//    public const string NodeTag = "principalIdentity";
//    public const string NodeKeyPrefix = "user:";

//    public static string GeneratedNodeKey() => RandomTool.GenerateRandomSequence(16);
//    public static string ToNodeKey(string principalId) => $"{NodeKeyPrefix}{principalId.NotEmpty().ToLower()}";
//    public static bool IsIdentity(string subject) => subject.NotEmpty().StartsWith(NodeKeyPrefix);
//    public static string RemoveNodeKeyPrefix(string subject) => subject.NotEmpty().StartsWith(NodeKeyPrefix) switch
//    {
//        false => subject,
//        true => subject[NodeKeyPrefix.Length..],
//    };

//    public static string ConstructEmailTag(string value) => $"email={value.ToLower()}";

//    public static string? ConstructLoginProviderTag(string? nameIdentifier)
//    {
//        return (nameIdentifier.IsNotEmpty()) switch
//        {
//            false => null,
//            true => $"loginProvider={nameIdentifier.ToLower()}",
//        };
//    }

//    public static Option<string> CreateQuery(this PrincipalIdentity user, ScopeContext context)
//    {
//        if (user.Validate().IsError(out var r)) return r.LogStatus(context, nameof(PrincipalIdentity)).ToOptionStatus<string>();

//        string nodeKey = IdentityTool.ToNodeKey(user.PrincipalId);

//        string emailTag = IdentityTool.ConstructEmailTag(user.Email);
//        string loginProviderTag = IdentityTool.ConstructLoginProviderTag(user.NameIdentifier) ?? "-loginProvider";

//        var cmd = new NodeCommandBuilder()
//            .UseSet()
//            .SetNodeKey(nodeKey)
//            .AddTag(IdentityTool.NodeTag)
//            .AddTag(emailTag)
//            .AddTag(loginProviderTag)
//            .AddData("entity", user)
//            .AddIndex("email")
//            .Action(x =>
//            {
//                if (user.HasLoginProvider()) x.AddIndex("loginProvider");
//            })
//            .Build();

//        return cmd;
//    }

//    public static CommandBatchBuilder AddIdentity(this CommandBatchBuilder builder, PrincipalIdentity subject)
//    {
//        builder.Add((context) => CreateQuery(subject, context));
//        return builder;
//    }

//    public static async Task<Option<bool>> GetEmailConfirmed(this IdentityClient client, string principalId, ScopeContext context)
//    {
//        var principalIdentity = await client.GetByPrincipalId(principalId, context);
//        if (principalIdentity.IsError()) return principalIdentity.ToOptionStatus<bool>();

//        return principalIdentity.Return().EmailConfirmed;
//    }

//    public static async Task<Option> SetEmailConfirmed(this IdentityClient client, string principalId, bool confirmed, ScopeContext context)
//    {
//        var principalIdentity = await client.GetByPrincipalId(principalId, context);
//        if (principalIdentity.IsError()) return principalIdentity.ToOptionStatus();

//        var identity = principalIdentity.Return() with { EmailConfirmed = confirmed };
//        var write = await client.Set(identity, context);
//        if (write.IsError()) return write.LogStatus(context, "Cannot set email confirmed for principalId={principalId}", [principalId]);

//        return write;
//    }
//}
