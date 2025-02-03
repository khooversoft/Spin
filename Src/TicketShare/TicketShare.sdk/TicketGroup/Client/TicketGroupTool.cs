using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Graph.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public static class TicketGroupTool
{
    internal const string NodeTag = "ticketGroup";
    public const string NodeKeyPrefix = "ticketGroup:";
    public const string EdgeType = "ticketGroup-user";

    public static string ToNodeKey(string securityGroupId) => $"{NodeKeyPrefix}{securityGroupId.NotEmpty().ToLower()}";

    public static string RemoveNodeKeyPrefix(string subject) => subject.NotEmpty().StartsWith(NodeKeyPrefix) switch
    {
        false => subject,
        true => subject[NodeKeyPrefix.Length..],
    };

    public static string ToTicketGroupHubChannelId(string ticketGroupId) => $"ticketGroup-channel/{ticketGroupId.NotEmpty().ToLowerInvariant()}";

    public static Option<string> CreateQuery(this TicketGroupRecord subject, bool useSet, IEnumerable<string> removeTagList, ScopeContext context)
    {
        if (subject.ChannelId.IsEmpty())
        {
            subject = subject with { ChannelId = ToTicketGroupHubChannelId(subject.TicketGroupId) };
        }

        if (subject.Validate().IsError(out var r)) return r.LogStatus(context, nameof(TicketGroupRecord)).ToOptionStatus<string>();

        var roles = subject.Roles
            .Select(x => x.PrincipalId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        string nodeKey = ToNodeKey(subject.TicketGroupId);

        var cmd = new NodeCommandBuilder()
            .UseSet(useSet)
            .SetNodeKey(nodeKey)
            .AddTag(NodeTag)
            .AddReferences(EdgeType, roles.Select(x => IdentityTool.ToNodeKey(x)))
            .Action(x => removeTagList.ForEach(y => x.AddTag("-" + x)))
            .AddData("entity", subject)
            .Build();

        return cmd;
    }

    public static CommandBatchBuilder AddTicketGroup(this CommandBatchBuilder builder, TicketGroupRecord subject, bool useSet)
    {
        builder.Add((context) => CreateQuery(subject, useSet, [], context));
        return builder;
    }

}
