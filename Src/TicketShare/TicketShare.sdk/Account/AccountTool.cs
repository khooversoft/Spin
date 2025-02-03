using Toolbox.Graph;
using Toolbox.Graph.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public static class AccountTool
{
    public const string NodeTag = "account";
    public const string NodeKeyPrefix = "account:";
    private const string EdgeType = "account-owns";

    public static string ToNodeKey(string principalId) => $"{NodeKeyPrefix}{principalId.NotEmpty().ToLower()}";

    public static Option<string> CreateQuery(this AccountRecord account, bool useSet, ScopeContext context)
    {
        if (account.Validate().IsError(out var r)) return r.LogStatus(context, nameof(AccountRecord)).ToOptionStatus<string>();

        string nodeKey = ToNodeKey(account.PrincipalId);

        var cmd = new NodeCommandBuilder()
            .UseSet(useSet)
            .SetNodeKey(nodeKey)
            .AddTag(NodeTag)
            .AddReference(EdgeType, IdentityTool.ToNodeKey(account.PrincipalId))
            .AddData("entity", account)
            .Build();

        return cmd;
    }

    public static CommandBatchBuilder AddAccount(this CommandBatchBuilder builder, AccountRecord subject, bool useSet)
    {
        builder.Add((context) => CreateQuery(subject, useSet, context));
        return builder;
    }
}
