using Toolbox.Extensions;
using Toolbox.Tools;

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
}
