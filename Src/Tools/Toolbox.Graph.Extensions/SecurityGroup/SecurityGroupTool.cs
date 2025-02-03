using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions;

public static class SecurityGroupTool
{
    public const string NodeTag = "securityGroup";
    public const string NodeKeyPrefix = "securityGroup:";
    public const string EdgeType = "securityGroup-user";

    public static string ToNodeKey(string securityGroupId) => $"{NodeKeyPrefix}{securityGroupId.NotEmpty().ToLower()}";

    public static string RemoveNodeKeyPrefix(string subject) => subject.NotEmpty().StartsWith(NodeKeyPrefix) switch
    {
        false => subject,
        true => subject[NodeKeyPrefix.Length..],
    };

    public static async Task<Option> HasAccess(IGraphClient client, string securityGroupId, string principalId, SecurityAccess accessRequired, ScopeContext context)
    {
        var subject = await client.GetNode<SecurityGroupRecord>(ToNodeKey(securityGroupId), context).ConfigureAwait(false);
        if (subject.IsError())
        {
            context.LogTrace("Security group not found, securityGroupId={securityGroupId}", securityGroupId);
            return subject.ToOptionStatus();
        }

        var read = subject.Return();
        if (read.HasAccess(principalId, accessRequired).IsError(out var status))
        {
            context.LogTrace("Access denied, securityGroupId={securityGroupId}, principalId={principalId}", securityGroupId, principalId);
            return status;
        }
        return StatusCode.OK;
    }

    public static Option<string> CreateQuery(this SecurityGroupRecord subject, bool useSet, ScopeContext context)
    {
        if (subject.Validate().IsError(out var r)) return r.LogStatus(context, nameof(SecurityGroupClient)).ToOptionStatus<string>();

        string nodeKey = SecurityGroupTool.ToNodeKey(subject.SecurityGroupId);

        var cmd = new NodeCommandBuilder()
            .UseSet(useSet)
            .SetNodeKey(nodeKey)
            .AddTag(SecurityGroupTool.NodeTag)
            .AddData("entity", subject)
            .AddReferences(
                SecurityGroupTool.EdgeType,
                subject.Members.Values.Select(x => GraphTool.ApplyIfRequired(x.PrincipalId, IdentityTool.ToNodeKey))
                )
            .Build();

        return cmd;
    }

    public static SecurityGroupRecord CreateRecord(string securityGroupId, string name, IEnumerable<(string user, SecurityAccess access)> access)
    {
        IEnumerable<PrincipalAccess> accessList = access.Select(x => new PrincipalAccess { PrincipalId = x.user, Access = x.access });
        return CreateRecord(securityGroupId, name, accessList);
    }

    public static SecurityGroupRecord CreateRecord(string securityGroupId, string name, IEnumerable<PrincipalAccess> access)
    {
        var subject = new SecurityGroupRecord
        {
            SecurityGroupId = securityGroupId.NotEmpty(),
            Name = name.NotEmpty(),
            Members = access.ToDictionary(x => x.PrincipalId, x => x, StringComparer.OrdinalIgnoreCase),
        };

        return subject;
    }
    public static CommandBatchBuilder AddSecurityGroup(this CommandBatchBuilder builder, SecurityGroupRecord subject, bool useSet)
    {
        builder.Add((context) => CreateQuery(subject, useSet, context));
        return builder;
    }
}
