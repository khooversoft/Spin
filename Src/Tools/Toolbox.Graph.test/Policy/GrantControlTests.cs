using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Graph.test.Policy;
public class GrantControlTests
{

    [Fact]
    public void GrantControl_Serialization_RoundTrip()
    {
        var groupPolicies = new[]
        {
            new GroupPolicy("group1", new[] { "user1", "user2" }),
            new GroupPolicy("group2", new[] { "user3", "user4" }),
        };

        var principals = new[]
        {
            new PrincipalIdentity("id1", "user:id1", "user1", "user1@domain.com", false),
            new PrincipalIdentity("id2", "user:id2", "user2", "user2@domain.com", true),
        };

        var grantControl = new GrantControl(groupPolicies, principals);

        // Act
        var json = grantControl.ToJson();
        var deserializedPolicy = json.ToObject<GrantControl>();

        // Assert
        (deserializedPolicy != default).BeTrue();
        (grantControl == deserializedPolicy).BeTrue();
    }

}
