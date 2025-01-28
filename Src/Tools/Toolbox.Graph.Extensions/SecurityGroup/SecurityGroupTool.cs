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
}
