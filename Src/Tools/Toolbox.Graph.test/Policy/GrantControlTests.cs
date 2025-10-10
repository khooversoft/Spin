using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Graph.test.Policy;
public class GrantControlTests
{

    [Fact]
    public void GrantControl_Serialization_RoundTrip()
    {
        // Arrange
        var originalPolicy = new GrantControl(new[]
        {
            new GrantPolicy("customerNumber", RolePolicy.Owner | RolePolicy.NameIdentifier, "user1orGroupName"),
            new GrantPolicy("customerNumber2", RolePolicy.Reader | RolePolicy.SecurityGroup, "user1orGroupName2"),
        },
        new[]
        {
            new GroupPolicy("group1", new[] { "user1", "user2" }),
            new GroupPolicy("group2", new[] { "user3", "user4" }),
        });

        // Act
        var json = originalPolicy.ToJson();
        var deserializedPolicy = json.ToObject<GrantControl>();

        // Assert
        (deserializedPolicy != default).BeTrue();
        (originalPolicy == deserializedPolicy).BeTrue();
    }

}
