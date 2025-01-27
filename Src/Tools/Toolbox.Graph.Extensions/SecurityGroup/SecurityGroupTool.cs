using Toolbox.Tools;

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
}
